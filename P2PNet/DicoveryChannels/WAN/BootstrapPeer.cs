using P2PNet.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.DicoveryChannels.WAN
    {
    /// <summary>
    /// Represents a bootstrap server using IPeer implementation.
    /// </summary>
    public class BootstrapPeer : IPeer
        {
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public TcpClient Client { get; set; }
        public NetworkStream Stream { get; set; }
        public string Identifier { get; set; }
        public BootstrapPeer() { }

        /// <summary>
        /// For staging communication with bootstrap server via REST-like API.
        /// </summary>
        /// <param name="URL">The URL of the bootstrap server.</param>
        public BootstrapPeer(string URL)
            {
            Identifier = URL;
            }

        /// <summary>
        /// For staging communication with bootstrap server via REST-like API.
        /// </summary>
        /// <param name="URL">The URL of the bootstrap server.</param>
        /// <param name="port">The port of the bootstrap server.</param>
        public BootstrapPeer(string URL, int port)
            {
            Identifier = URL;
            Port = port;
            }

        /// <summary>
        /// For establishing a TCP connection to a bootstrap server.
        /// </summary>
        /// <param name="ip">The IP address of the bootstrap server.</param>
        /// <param name="port">The port of the bootstrap server.</param>
        public BootstrapPeer(IPAddress ip, int port)
            {
            IP = ip;
            Port = port;
            }
        }
    }
