using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MUA.Server.TCP.Telnet
{
    /// <summary>
    ///     <todo>Make an interface that enforces Metadata and Startup.</todo>
    /// </summary>
    internal class TelnetServer : TCPServer
    {
        /// <summary>
        ///     All telnet connections carry this Metadata. This is for the TCPServer parent.
        /// </summary>
        private const string Metadata = "telnet";

        public TelnetServer(IPAddress address, Int32 port, ref TCPInitializer.ClientList clientlist)
            : base(address, port, ref clientlist)
        {
        }

        /// <summary>
        ///     This code is here to handle negociation, and storing the negociation information
        ///     on the client object using its KeyValuePair storage.
        ///     WARNING! This code assumes that negociation is clean, and does not fail.
        ///     This really should be written in the form of a state-machine.
        ///     But as this is just a proof of concept, this will do for now.
        ///     <todo>
        ///         Secondary Warning, we seem to have OFF-BY-ONE issues with TTYPE negocation.
        ///         That should not be happening! I need to look into how to fix this!
        ///     </todo>
        /// </summary>
        /// <param name="client">A TCP Client reference.</param>
        /// <param name="bytes">The bytes of data that have come through.</param>
        /// <param name="length">How long we can scan through this array.</param>
        /// <returns></returns>
        private int Negociate(ref TCPInitializer.Client client, ref Byte[] bytes, int length)
        {
            int i;
            Console.WriteLine("NEGOCIATION!");
            var notes = new List<string>();
            var stream = client.client.GetStream();

            for (i = 0; i < length; i++)
            {
                /*
                 *  WILL NEGOCIATION
                 */
                if (bytes[i] == (byte) NegociationOptions.WILL)
                {
                    if (length > i + 1)
                    {
                        if (bytes[i + 1] == (byte) NegociationOptions.NAWS)
                        {
                            // Do nothing. They will send us stuff.
                        }
                        else if (bytes[i + 1] == (byte) NegociationOptions.NEWENVIRON)
                        {
                            Console.WriteLine("They have indicated WILL NEWENVIRON. Sending negociation string.");

                            // We Send: IAC SB NEW-ENVIRON SEND IAC SE
                            foreach (byte tosend in new List<NegociationOptions>
                            {
                                NegociationOptions.IAC,
                                NegociationOptions.SB,
                                NegociationOptions.NEWENVIRON,
                                NegociationOptions.SEND,
                                NegociationOptions.IAC,
                                NegociationOptions.SE
                            })
                            {
                                stream.WriteByte(tosend);
                            }
                        }
                        else if (bytes[i + 1] == (byte) NegociationOptions.TTYPE)
                        {
                            Console.WriteLine("They have indicated WILL TTYPE. Sending negociation string.");
                            // We Send: IAC SB TERMINAL-TYPE SEND IAC SE
                            foreach (byte tosend in new List<NegociationOptions>
                            {
                                NegociationOptions.IAC,
                                NegociationOptions.SB,
                                NegociationOptions.TTYPE,
                                NegociationOptions.SEND,
                                NegociationOptions.IAC,
                                NegociationOptions.SE
                            })
                            {
                                stream.WriteByte(tosend);
                            }
                        }
                        else if (bytes[i + 1] == (byte) NegociationOptions.CHARSET)
                        {
                            Console.WriteLine("CHARSET NEG? (WILL). Let's try sending SB SEND");
                            var charsetsupport = Encoding.ASCII.GetBytes(";UTF-8;US-ASCII");
                            stream.WriteByte((byte) NegociationOptions.IAC);
                            stream.WriteByte((byte) NegociationOptions.SB);
                            stream.WriteByte((byte) NegociationOptions.CHARSET);
                            stream.WriteByte((byte) NegociationOptions.REQUEST);
                            stream.Write(charsetsupport, 0, charsetsupport.Length);
                            stream.WriteByte((byte) NegociationOptions.IAC);
                            stream.WriteByte((byte) NegociationOptions.SE);
                        }
                        else
                        {
                            Console.WriteLine("ERROR! The client WILL do something we don't recognize!");
                            // Gibberish it is! Jerk.
                            return i;
                        }
                    }
                }

                /*
                 *  SUB NEGOCIATION - THIS LARGELY IS US INTERPRETING INCOMING NEGOCIATION
                 */
                else if (bytes[i] == (byte) NegociationOptions.SB)
                {
                    // Begin Subnegociation - this comes after an IAC
                    // After this, everything is negociation!

                    if (length > i + 1)
                    {
                        /*
                         *  NAWS NEGOCIATION
                         */
                        if (bytes[i + 1] == (byte) NegociationOptions.NAWS && length > i + 7)
                        {
                            // We really should move this code to the Client creation.
                            // A client should always have these two. 
                            if (!client.KeyValuePairs.ContainsKey("WIDTH"))
                                client.KeyValuePairs.Add("WIDTH", "78");
                            if (!client.KeyValuePairs.ContainsKey("HEIGHT"))
                                client.KeyValuePairs.Add("HEIGHT", "24");

                            // We are expecting: 
                            // IS <INT as Width> IS <INT as Height>
                            // Then IAC SE
                            if (bytes[i + 2] != (byte) NegociationOptions.IS) break;
                            client.KeyValuePairs["WIDTH"] = bytes[i + 3].ToString();
                            if (bytes[i + 4] != (byte) NegociationOptions.IS) break;
                            client.KeyValuePairs["HEIGHT"] = bytes[i + 5].ToString();
                            if (bytes[i + 6] != (byte) NegociationOptions.IAC) break;
                            if (bytes[i + 7] != (byte) NegociationOptions.SE) break;
                            i = i + 7;
                            Console.WriteLine("The client has indicated WIDTH = " + client.KeyValuePairs["WIDTH"]
                                              + " HEIGHT = " + client.KeyValuePairs["HEIGHT"]);
                        }

                        /*
                         *  ENVIRONMENT NEGOCIATION
                         */
                        else if (bytes[i + 1] == (byte) NegociationOptions.NEWENVIRON)
                        {
                        }

                        /*
                         *  TERMINAL TYPE NEGOCIATION
                         */
                        else if (bytes[i + 1] == (byte) NegociationOptions.TTYPE)
                        {
                            // We are expecting: 
                            // IS < other things > IAC SE
                            // We CAN then send another request for a TTYPE, to enumerate, until we get a duplicate.
                            // At which point we stop!
                            if (bytes[i + 2] != (byte) NegociationOptions.IS) break;
                            var TermType = new StringBuilder();
                            var completed = false;
                            for (i = i + 3; i < length; i++)
                            {
                                if (bytes[i] == (byte) NegociationOptions.IAC &&
                                    length > i + 1 &&
                                    bytes[i + 1] == (byte) NegociationOptions.SE)
                                {
                                    if (!client.KeyValuePairs.ContainsKey("TERMTYPE"))
                                    {
                                        client.KeyValuePairs.Add("TERMTYPE", TermType.ToString());
                                    }
                                    else
                                    {
                                        client.KeyValuePairs["TERMTYPE"] = " " + TermType;
                                    }
                                    completed = true;
                                    i++;
                                }
                                else
                                {
                                    TermType.Append((char) bytes[i]);
                                }
                            }
                            if (!completed)
                                Console.WriteLine("INCOMPLETE TERMTYPE! HOW DO WE RECOVER?!");
                            else
                            {
                                Console.WriteLine("TERMTYPE: " + client.KeyValuePairs["TERMTYPE"] + " ");
                            }
                        }

                        /*
                         *  CHARACTER SET NEGOCIATION
                         */
                        else if (bytes[i + 1] == (byte) NegociationOptions.CHARSET)
                        {
                            var charsetnegociation = new StringBuilder();
                            for (i = i + 1; i < length; i++)
                            {
                                if (bytes[i] == (byte) NegociationOptions.IAC &&
                                    length > i + 1 &&
                                    bytes[i + 1] == (byte) NegociationOptions.SE)
                                {
                                    Console.WriteLine(charsetnegociation);
                                    i++;
                                    break;
                                }
                                switch (bytes[i])
                                {
                                    case (byte) NegociationOptions.ACCEPTED:
                                        charsetnegociation.Append(NegociationOptions.ACCEPTED + " ");
                                        break;
                                    case (byte) NegociationOptions.REQUEST:
                                        charsetnegociation.Append(NegociationOptions.REQUEST + " ");
                                        break;
                                    case (byte) NegociationOptions.CHARSET:
                                        charsetnegociation.Append(NegociationOptions.CHARSET + " ");
                                        break;
                                    case (byte) NegociationOptions.REJECTED:
                                        charsetnegociation.Append(NegociationOptions.REJECTED + " ");
                                        break;
                                    case (byte) NegociationOptions.TTABLE_ACK:
                                        charsetnegociation.Append(NegociationOptions.TTABLE_ACK + " ");
                                        break;
                                    case (byte) NegociationOptions.TTABLE_IS:
                                        charsetnegociation.Append(NegociationOptions.TTABLE_IS + " ");
                                        break;
                                    case (byte) NegociationOptions.TTABLE_NAK:
                                        charsetnegociation.Append(NegociationOptions.TTABLE_NAK + " ");
                                        break;
                                    case (byte) NegociationOptions.TTABLE_REJECTED:
                                        charsetnegociation.Append(NegociationOptions.TTABLE_REJECTED + " ");
                                        break;
                                    default:
                                        charsetnegociation.Append((char) bytes[i]);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("ERROR! The client WILL do something we don't recognize!");
                        }
                    }
                }
            }
            return i;
        }

        /// <summary>
        ///     Startup the server.
        ///     <todo>Convert to a Task?</todo>
        /// </summary>
        public override void Startup()
        {
            try
            {
                Start();
                // Buffer for reading data
                var bytes = new Byte[2097152]; // 2 megabytes large. This is in the case of POST data!
                String data = null;

                // Enter the listening loop. 
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    var client = new TCPInitializer.Client(AcceptTcpClient(), Metadata);
                    AllClients.Add(client);

                    data = null;

                    // Get a stream object for reading and writing
                    var stream = client.client.GetStream();

                    int i;
                    int negstart;

                    // Let's tell it what we're willing to do here!
                    // We do NAWS, CHARSET, TTYPE and NEWENVIRON
                    foreach (var option in new[]
                    {
                        NegociationOptions.NAWS,
                        NegociationOptions.CHARSET,
                        NegociationOptions.TTYPE,
                        NegociationOptions.NEWENVIRON
                    })
                    {
                        stream.WriteByte((byte) NegociationOptions.IAC);
                        stream.WriteByte((byte) NegociationOptions.DO);
                        stream.WriteByte((byte) option);
                    }

                    // Loop to receive all the data sent by the client. 
                    while ((i = stream.Read(client.Bytes, client.BytePtr, client.Bytes.Length-client.BytePtr)) != 0)
                    {
                        negstart = client.BytePtr;
                        // If the first character is telnet IAC, and it is not an escaped char 255...
                        if (i > 1 && client.Bytes[0] == 255 && client.Bytes[1] != 255)
                        {
                            negstart = Negociate(ref client, ref client.Bytes, i);
                            Console.WriteLine("Negociation was {0} characters of {1} bytes.", negstart, i);
                            if (negstart >= i)
                            {
                                Console.WriteLine("This was pure negociation. Going to the next message.");
                                continue;
                            }
                        }


                        var bytesgotten = new StringBuilder();

                        for (var j = 0; j < i; j++)
                        {
                            bytesgotten.Append(client.Bytes[j]);
                            bytesgotten.Append(" ");
                        }

                        // Translate data bytes to a ASCII string.
                        data = Encoding.ASCII.GetString(client.Bytes, negstart, i - negstart);
                        Console.WriteLine("Received: {0} AKA {1}", data, bytesgotten);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        var msg = Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        // stream.Write(msg, 0, msg.Length);
                        // Console.WriteLine("Sent: {0}", data);
                        if ( i > 2 && client.BytePtr > 2 )
                        {
                            if((client.Bytes[client.BytePtr - 1] == (byte) NegociationOptions.NEWLINE
                             && client.Bytes[client.BytePtr] == (byte) NegociationOptions.CR) ||
                            (client.Bytes[client.BytePtr - 1] == (byte) NegociationOptions.CR
                             && client.Bytes[client.BytePtr] == (byte) NegociationOptions.NEWLINE))
                            {
                                // client.Bytes.Initialize();
                                client.BytePtr = 0;
                            }
                        }
                    }

                    // Shutdown and end connection
                    AllClients.Disconnect(client);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                Stop();
                AllClients.Disconnect(Metadata);
            }


            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        /// <summary>
        ///     The Negociation Options, for translating from human readable to decimal notation.
        /// </summary>
        private enum NegociationOptions
        {
            IAC = 255,
            WILL = 251,
            WONT = 252,
            DO = 253,
            DONT = 254,
            NOP = 241,
            SB = 250,
            SE = 240,
            AYT = 246,
            LINEMODE = 34,
            NEWLINE = 10,
            CR = 13,
            SGA = 3,
            // Charset Options
            CHARSET = 42,
            REQUEST = 01,
            ACCEPTED = 02,
            REJECTED = 03,
            TTABLE_IS = 04,
            TTABLE_REJECTED = 05,
            TTABLE_ACK = 06,
            TTABLE_NAK = 07,
            // Terminal Type Options
            TTYPE = 24,
            // Window Size Options
            NAWS = 31,
            // New Environmental Options
            NEWENVIRON = 39,
            SEND = 1,
            IS = 0,
            INFO = 2,
            VAR = 0,
            VALUE = 1,
            ESC = 2,
            USERVAR = 3,
            // MSSP Options
            MSSP = 70,
            MSSP_VAL = 2,
            MSSP_NAME = 1
        }
    }
}