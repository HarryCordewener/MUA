﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MUA.Server.TCP
{
    /// <summary>
    /// Parent for the TCP connections MUA can make.
    /// </summary>
    class TCPServer : TcpListener
    {
        /// <summary>
        /// Lists the clients connected to this medium.
        /// </summary>
        protected TCPInitializer.ClientList allClients;


        public TCPServer(IPAddress address, Int32 port, ref TCPInitializer.ClientList clientlist) : base(address, port)
        {
            allClients = clientlist;
        }

        /// <summary>
        /// <todo>Interface this!</todo>
        /// </summary>
        public virtual void Startup() {}
    }
}
