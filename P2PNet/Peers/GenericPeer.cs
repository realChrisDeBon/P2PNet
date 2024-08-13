using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using P2PNet.Peers;

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

        /// <summary>
        /// Gets the PeerChannel associated with this GenericPeer.
        /// </summary>
        /// <returns>PeerChannel associated with this GenericPeer.</returns>
        public PeerChannel GetPeerChannel()
            {
            foreach(PeerChannel channel_ in PeerNetwork.ActivePeerChannels)
                {
                if(channel_.peer == this)
                    {
                    return channel_;
                    break;
                    }
                }
            return null; // no channel found
            }

        }
    }
