using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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
        ///     This code is here to handle Negotiation, and storing the Negotiation information
        ///     on the client object using its KeyValuePair storage.
        ///     WARNING! This code assumes that Negotiation is clean, and does not fail.
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
            Console.WriteLine("Negotiation!");
            var notes = new List<string>();
            var stream = client.client.GetStream();

            for (i = 0; i < length; i++)
            {
                /*
                 *  WILL Negotiation
                 */
                if (bytes[i] == (byte) NegotiationOptions.WILL)
                {
                    if (length > i + 1)
                    {
                        if (bytes[i + 1] == (byte) NegotiationOptions.NAWS)
                        {
                            // Do nothing. They will send us stuff.
                        }
                        else if (bytes[i + 1] == (byte) NegotiationOptions.NEWENVIRON)
                        {
                            Console.WriteLine("They have indicated WILL NEWENVIRON. Sending Negotiation string.");

                            // We Send: IAC SB NEW-ENVIRON SEND IAC SE
                            foreach (var tosend in new List<NegotiationOptions>
                            {
                                NegotiationOptions.IAC,
                                NegotiationOptions.SB,
                                NegotiationOptions.NEWENVIRON,
                                NegotiationOptions.SEND,
                                NegotiationOptions.IAC,
                                NegotiationOptions.SE
                            })
                            {
                                stream.WriteByte((byte) tosend);
                            }
                        }
                        else if (bytes[i + 1] == (byte) NegotiationOptions.TTYPE)
                        {
                            Console.WriteLine("They have indicated WILL TTYPE. Sending Negotiation string.");
                            // We Send: IAC SB TERMINAL-TYPE SEND IAC SE
                            foreach (var tosend in new List<NegotiationOptions>
                            {
                                NegotiationOptions.IAC,
                                NegotiationOptions.SB,
                                NegotiationOptions.TTYPE,
                                NegotiationOptions.SEND,
                                NegotiationOptions.IAC,
                                NegotiationOptions.SE
                            })
                            {
                                stream.WriteByte((byte) tosend);
                            }
                        }
                        else if (bytes[i + 1] == (byte) NegotiationOptions.CHARSET)
                        {
                            Console.WriteLine("CHARSET NEG? (WILL). Let's try sending SB SEND");
                            var charsetsupport = Encoding.ASCII.GetBytes(";UTF-8;US-ASCII");
                            stream.WriteByte((byte) NegotiationOptions.IAC);
                            stream.WriteByte((byte) NegotiationOptions.SB);
                            stream.WriteByte((byte) NegotiationOptions.CHARSET);
                            stream.WriteByte((byte) NegotiationOptions.REQUEST);
                            stream.Write(charsetsupport, 0, charsetsupport.Length);
                            stream.WriteByte((byte) NegotiationOptions.IAC);
                            stream.WriteByte((byte) NegotiationOptions.SE);
                        }
                        else
                        {
                            Console.WriteLine("ERROR! The client WILL do something we don't recognize!");
                            return i; // Bail!
                        }
                    }
                }

                /*
                 *  SUB Negotiation - THIS LARGELY IS US INTERPRETING INCOMING Negotiation
                 */
                else if (bytes[i] == (byte) NegotiationOptions.SB)
                {
                    // Begin SubNegotiation - this comes after an IAC
                    // After this, everything is Negotiation!

                    if (length > i + 1)
                    {
                        /*
                         *  NAWS Negotiation
                         */
                        if (bytes[i + 1] == (byte) NegotiationOptions.NAWS && length > i + 7)
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
                            if (bytes[i + 2] != (byte) NegotiationOptions.IS) break;
                            client.KeyValuePairs["WIDTH"] = bytes[i + 3].ToString();
                            if (bytes[i + 4] != (byte) NegotiationOptions.IS) break;
                            client.KeyValuePairs["HEIGHT"] = bytes[i + 5].ToString();
                            if (bytes[i + 6] != (byte) NegotiationOptions.IAC) break;
                            if (bytes[i + 7] != (byte) NegotiationOptions.SE) break;
                            i = i + 7;
                            Console.WriteLine("The client has indicated WIDTH = " + client.KeyValuePairs["WIDTH"]
                                              + " HEIGHT = " + client.KeyValuePairs["HEIGHT"]);
                        }

                        /*
                         *  ENVIRONMENT Negotiation
                         */
                        else if (bytes[i + 1] == (byte) NegotiationOptions.NEWENVIRON)
                        {
                            // We don't know yet, just how to do this.
                        }

                        /*
                         *  TERMINAL TYPE Negotiation
                         */
                        else if (bytes[i + 1] == (byte) NegotiationOptions.TTYPE)
                        {
                            // We are expecting: 
                            // IS < other things > IAC SE
                            // We CAN then send another request for a TTYPE, to enumerate, until we get a duplicate.
                            // At which point we stop!
                            if (bytes[i + 2] != (byte) NegotiationOptions.IS) break;
                            var termType = new StringBuilder();
                            var completed = false;
                            for (i = i + 3; i < length; i++)
                            {
                                if (bytes[i] == (byte) NegotiationOptions.IAC &&
                                    length > i + 1 &&
                                    bytes[i + 1] == (byte) NegotiationOptions.SE)
                                {
                                    if (!client.KeyValuePairs.ContainsKey("TERMTYPE"))
                                    {
                                        client.KeyValuePairs.Add("TERMTYPE", termType.ToString());
                                    }
                                    else
                                    {
                                        if (!client.KeyValuePairs["TERMTYPE"].Contains(termType.ToString()))
                                            client.KeyValuePairs["TERMTYPE"] = " " + termType;
                                    }
                                    completed = true;
                                }
                                else
                                {
                                    termType.Append((char) bytes[i]);
                                }
                            }
                            if (!completed)
                            {
                                Console.WriteLine("INCOMPLETE TERMTYPE! HOW DO WE RECOVER?!");
                                return 0; // Bail
                            }
                            Console.WriteLine("TERMTYPE: " + client.KeyValuePairs["TERMTYPE"] + " ");
                            return i;
                        }

                        /*
                         *  CHARACTER SET Negotiation
                         */
                        else if (bytes[i + 1] == (byte) NegotiationOptions.CHARSET)
                        {
                            var charsetNegotiation = new StringBuilder();
                            for (i = i + 1; i < length; i++)
                            {
                                if (bytes[i] == (byte) NegotiationOptions.IAC &&
                                    length > i + 1 &&
                                    bytes[i + 1] == (byte) NegotiationOptions.SE)
                                {
                                    Console.WriteLine(charsetNegotiation);
                                    i++;
                                    break;
                                }
                                switch (bytes[i])
                                {
                                    case (byte) NegotiationOptions.ACCEPTED:
                                        charsetNegotiation.Append(NegotiationOptions.ACCEPTED + " ");
                                        break;
                                    case (byte) NegotiationOptions.REQUEST:
                                        charsetNegotiation.Append(NegotiationOptions.REQUEST + " ");
                                        break;
                                    case (byte) NegotiationOptions.CHARSET:
                                        charsetNegotiation.Append(NegotiationOptions.CHARSET + " ");
                                        break;
                                    case (byte) NegotiationOptions.REJECTED:
                                        charsetNegotiation.Append(NegotiationOptions.REJECTED + " ");
                                        break;
                                    case (byte) NegotiationOptions.TTABLE_ACK:
                                        charsetNegotiation.Append(NegotiationOptions.TTABLE_ACK + " ");
                                        break;
                                    case (byte) NegotiationOptions.TTABLE_IS:
                                        charsetNegotiation.Append(NegotiationOptions.TTABLE_IS + " ");
                                        break;
                                    case (byte) NegotiationOptions.TTABLE_NAK:
                                        charsetNegotiation.Append(NegotiationOptions.TTABLE_NAK + " ");
                                        break;
                                    case (byte) NegotiationOptions.TTABLE_REJECTED:
                                        charsetNegotiation.Append(NegotiationOptions.TTABLE_REJECTED + " ");
                                        break;
                                    default:
                                        charsetNegotiation.Append((char) bytes[i]);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("ERROR! The client WILL do something we don't recognize!");
                            return i; // Bail!
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
                var bytes = new Byte[MUASettings.Default.client_buffer_len * 2]; 

                // Enter the listening loop. 
                while (true)
                {
                    Console.Write("Waiting for a connection... ");
                    // Perform a blocking call to accept requests. 
                    // You could also user server.AcceptSocket() here.
                    var client = new TCPInitializer.Client(AcceptTcpClient(), Metadata);
                    AllClients.Add(client);

                    // Get a stream object for reading and writing
                    var stream = client.client.GetStream();

                    int i;
                    int concat = 0;

                    // Let's tell it what we're willing to do here!
                    // We do NAWS, CHARSET, TTYPE and NEWENVIRON
                    foreach (var option in new[]
                    {
                        NegotiationOptions.NAWS,
                        NegotiationOptions.CHARSET,
                        NegotiationOptions.TTYPE,
                        NegotiationOptions.NEWENVIRON
                    })
                    {
                        stream.WriteByte((byte) NegotiationOptions.IAC);
                        stream.WriteByte((byte) NegotiationOptions.DO);
                        stream.WriteByte((byte) option);
                    }

                    // Loop to receive all the data sent by the client. 
                    while ((i = stream.Read(bytes, concat, MUASettings.Default.client_buffer_len)) != 0)
                    {
                        int c = 0;
                        int postNegotiationPtr = 0;

                        // We refuse to believe IAC will appear anywhere BUT at the start of a sent.
                        // We also refuse to believe a single negociation string will be longer than the default client
                        // buffer length, which hopefully will be about 2 MB or more.
                        // If the first character is telnet IAC, and it is not an escaped char 255...
                        if (i > 1 && bytes[0] == 255 && bytes[1] != 255 && concat == 0)
                        {
                            postNegotiationPtr = Negociate(ref client, ref bytes, i);
                            Console.WriteLine("Negotiation was {0} characters of {1} bytes.", postNegotiationPtr, i);
                            if (postNegotiationPtr > i)
                            {
                                for (int bte = 0; bte < i; bte++)
                                {
                                    Console.Write(bytes[bte] + " ");
                                }
                                Console.WriteLine("\nWas received");
                            }
                            if (postNegotiationPtr >= i)
                            {
                                Console.WriteLine("This was pure Negotiation. Going to the next message.");
                                continue;
                            }
                        }

                        /*
                         * We need a way here to 'confirm' how many we received and actually copied over.
                         * But post-negociation, we can at least attempt to copy as much as we have.
                         * We then look through 'bytes' for a \n\r or \r\n, so we can treat it as a 'send'
                         * and clear our buffer? Or should we, upon reaching the max length, just assume that
                         * was a send and toss the rest we received to the wayside?
                         * The latter most certainly would be the easiest way to go about it!
                         */
                        for (var b = client.BytePtr; (b < client.Bytes.Length && c < i); b++, c++)
                        {
                            client.Bytes[b] = bytes[c + concat];
                        }
                        Console.WriteLine("Inserting at pointer position {0}", client.BytePtr);
                        Console.WriteLine("We received {0} bytes, and recorded {1} of those.", i, c);


                        var bytesReceived = new StringBuilder();

                        for (var j = 0; (j < client.BytePtr + i && j < client.Bytes.Length); j++)
                        {
                            bytesReceived.Append(client.Bytes[j]);
                            bytesReceived.Append(" ");
                        }

                        // Translate data bytes to a ASCII string.
                        var data = Encoding.ASCII.GetString(client.Bytes, postNegotiationPtr,
                            Math.Min(client.BytePtr + i - postNegotiationPtr, client.Bytes.Length));
                        Console.WriteLine("Received: {0} AKA {1}", data, bytesReceived);

                        var msg = Encoding.ASCII.GetBytes("Buffer cleared.");

                        // Send back a response.
                        // stream.Write(msg, 0, msg.Length);
                        // Console.WriteLine("Sent: {0}", data);

                        client.BytePtr = Math.Min(client.BytePtr + i, client.Bytes.Length);

                        if (i + concat >= 2 &&
                            ((bytes[i - 1 + concat] == (byte)NegotiationOptions.NEWLINE
                              && bytes[i - 1 - 1 + concat] == (byte)NegotiationOptions.CR) ||
                             (bytes[i - 1 + concat] == (byte)NegotiationOptions.CR
                              && bytes[i - 1 - 1 + concat] == (byte)NegotiationOptions.NEWLINE)))
                        {
                            // client.Bytes.Initialize();
                            client.BytePtr = 0;
                            Console.WriteLine("Buffer cleared.");
                            client.client.GetStream().Write(msg,0,msg.Length);
                            /*
                             * THIS IS WHERE WE MUST SEND THE INFORMATION TO THE PARSER!
                             * WE QUEUE THE CLIENT BUFFER ONTO THE PARSE STACK.
                             */
                            concat = 0;
                        }
                        else
                        {
                            // We are not re-using, to make the code more readable. Concat has a very specific meaning.
                            concat = client.Bytes.Length;
                            for (int block = client.Bytes.Length; block < 2 * client.Bytes.Length; block++)
                            {
                                bytes[block - client.Bytes.Length] = bytes[block];
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
        ///     The Negotiation Options, for translating from human readable to decimal notation.
        /// </summary>
        private enum NegotiationOptions
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