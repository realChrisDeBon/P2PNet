using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace P2PNet.NetworkPackets { 
    /// <summary>
    /// Standard packet used to relay identifying information through out peer network.
    /// </summary>
    public sealed class IdentifierPacket : INetworkPacket
        {
        /// <summary>
        /// Optional data or information to assist in establishing network connection.
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// Relevant data to assist in establishing network connection.
        /// </summary>
        public int Data { get; set; }
        /// <summary>
        /// IP address to broadcast.
        /// </summary>
        public string IP { get; set; }
        public IdentifierPacket() { }
        public IdentifierPacket(string message, int data, IPAddress ip_)
        {
            Message = message;
            Data = data;
            IP = ip_.ToString();
        }
    }
}