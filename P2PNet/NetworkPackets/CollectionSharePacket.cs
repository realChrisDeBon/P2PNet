using P2PNet.Peers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace P2PNet.NetworkPackets
{
    /// <summary>
    /// Standard packet used to relay a collection of peer-identifying information through out a network.
    /// </summary>
    public class CollectionSharePacket
    {
        public int Data { get; set; }
        public List<IPeer> peers { get; set; } = new List<IPeer>();
        public CollectionSharePacket() { }
        public CollectionSharePacket(int data, List<IPeer> peers)
            {
            this.Data = data;
            this.peers = peers;
            }
    }
}