using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using MUA.Server.TCP.Telnet;

namespace MUA.Server.TCP
{
    /// <summary>
    /// The true TCP class that negociates and collates all the various TCP frontends running.
    /// This TCP class is what the MUSH interacts with. Beyond this, all interactions must be done by the Server 
    /// children. This means that ansi-parsing and other markup will have to be written per server for sending.
    /// </summary>
    public class TCPInitializer
    {
        private ClientList clients;
        private List<Server> servers; 

        /// <summary>
        /// A client is defined by its TCP connection, its type, and other various information
        /// stored within its KeyValuePairs.
        /// </summary>
        public class Client
        {
            public Byte[] Bytes;
            public int BytePtr;
            public TcpClient client;
            public string Metadata;
            public Dictionary<String,String> KeyValuePairs; 

            public Client(TcpClient client, string metadata)
            {
                // Bytes = new Byte[2097152]; // Send buffer, for Windows connects etc.
                Bytes = new Byte[MUASettings.Default.client_buffer_len]; // Send buffer, for Windows connects etc.
                BytePtr = 0; // Assists with the buffer, pointing at our location.
                this.client = client;
                Metadata = metadata;
                KeyValuePairs = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// The list of clients connected to the MUA as a whole.
        /// </summary>
        public class ClientList
        {
            private List<Client> clients;

            public ClientList()
            {
                clients = new List<Client>();
            }

            /// <summary>
            /// Disconnect a client, removing it from our list.
            /// </summary>
            /// <param name="client">A client class object.</param>
            public void Disconnect(Client client)
            {
                client.client.Close();
                clients.Remove(client);
            }

            /// <summary>
            /// This disconnects all clients of a specific Metatype.
            /// This is to be executed when one type of server goes down, but not another.
            /// Example: The Telnet server crashes, but the HTTP server remains operational.
            /// </summary>
            /// <param name="type">A string that matches the exact Metadata type of the client types we wish to disconnect.</param>
            public void Disconnect(string type)
            {
                List<Client> toDisconnectList = clients.FindAll(client => client.Metadata == type);
                foreach (var client in toDisconnectList)
                {
                    client.client.Close();
                }
                clients.RemoveAll(client => client.Metadata == type);
            }

            /// <summary>
            /// Add a client to the listing. The client (stream) is expected to be open!
            /// </summary>
            /// <param name="client">A client class object.</param>
            public void Add(Client client)
            {
                clients.Add(client);
            }
        }

        /// <summary>
        /// <todo>Look into the wide usage of the Metadata attribute. It needs to be centralized.</todo>
        /// </summary>
        class Server
        {
            public readonly TCPServer server;
            private string Metadata;

            public Server(IPAddress addr, Int32 port, string metadata, ref ClientList clients)
            {
                Metadata = metadata;

                // We need better handling for this.
                switch (metadata)
                {
                    case "telnet":
                        server = new TelnetServer(addr, port, ref clients);
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes the TCPInitializer class.
        /// </summary>
        public TCPInitializer()
        {
            clients = new ClientList();
            servers = null;
        }

        /// <summary>
        /// Allows us to start up all the servers.
        /// <todo>Rewrite this to use a configuration from XML or similar.</todo>
        /// </summary>
        /// <returns></returns>
        private List<Server> GetServers()
        {
            const int port = 9090;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");
            Server telnetServer = new Server(localAddr, port, "telnet", ref clients);
            return new List<Server> {telnetServer};
        }

        /// <summary>
        /// This calls the Startup() sequence on all Servers.
        /// </summary>
        public void Serve()
        {
            servers = GetServers();
            foreach (var server in servers)
            {
                server.server.Startup();
            }
        }
    }
}