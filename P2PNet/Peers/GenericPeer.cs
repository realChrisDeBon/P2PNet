using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Peers
    {
    /// <summary>
    /// Represents a default peer implementation using IPeer.
    /// </summary>
    public class GenericPeer : IPeer
        {
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string Identifier { get; set; }
        public GenericPeer() { }

        public GenericPeer(IPAddress ip, int port)
            {
            IP = ip;
            Port = port;
            }

        }
    }
