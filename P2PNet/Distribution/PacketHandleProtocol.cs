using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace P2PNet.Distribution
{
    namespace P2PNet.Distribution
    {
        /// <summary>
        /// Provides protocol actions for handling different types of packets received by <see cref="Peers.PeerChannel"/> instances.
        /// </summary>
        /// <remarks>When a <see cref="PeerChannel"/> receives an inbound packet, it will invoke the respective Action while passing the data to the delegate.</remarks>
        public static class PacketHandleProtocol
        {
            /// <summary>
            /// Action to handle identity packets.
            /// </summary>
            public static Action<string> HandleIdentityPacketAction { get; set; } = HandleIdentityPacket;
            private static void HandleIdentityPacket(string packet)
            {
                var identityPacket = Deserialize<IdentifierPacket>(packet);
                if (identityPacket == null) { return; }
                else
                {
                    IPeer newPeer = new GenericPeer(IPAddress.Parse(identityPacket.IP), identityPacket.Data);
                    PeerNetwork.AddPeer(newPeer);
                }
            } // default behavior

            /// <summary>
            /// Action to handle disconnect packets.
            /// </summary>
            public static Action<string> HandleDisconnectPacketAction { get; set; } = HandleDisconnectPacket;
            private static void HandleDisconnectPacket(string packet)
            {
                var disconnectPacket = Deserialize<DisconnectPacket>(packet);
                if (disconnectPacket == null) { return; }
                else
                {
                    string ip = disconnectPacket.IP;
                    string port = disconnectPacket.Port.ToString();

                    var peerChannel = PeerNetwork.ActivePeerChannels.FirstOrDefault(x => x.peer.IP.ToString() == ip && x.peer.Port.ToString() == port);
                    if (peerChannel == null)
                    {
                        var peer = PeerNetwork.KnownPeers.FirstOrDefault(x => x.IP.ToString() == ip && x.Port.ToString() == port);
                        if (peer != null)
                        {
                            PeerNetwork.KnownPeers.Remove(peerChannel.peer);
                        }
                    }
                    else
                    {
                        PeerNetwork.RemovePeer(peerChannel);
                    }
                }
            } // default behavior

            /// <summary>
            /// Action to handle peer group packets.
            /// </summary>
            public static Action<string> HandlePeerGroupPacketAction { get; set; } = HandlePeerGroupPacket;
            private static void HandlePeerGroupPacket(string packet)
            {
                var colsharepkt = Deserialize<CollectionSharePacket>(packet);
                if (colsharepkt != null)
                {
                    PeerNetwork.ProcessPeerList(colsharepkt);
                }
            } // default behavior

            /// <summary>
            /// Action to handle data transmission packets.
            /// </summary>
            public static Action<string> HandleDataTransmissionPacketAction { get; set; } = HandleDataTransmissionPacket;
            private static void HandleDataTransmissionPacket(string packet)
            {
                DistributionHandler.EnqueueIncomingDataPacket(packet);
            } // default behavior

            /// <summary>
            /// Action to handle pure message packets.
            /// </summary>
            public static Action<string> HandlePureMessagePacketAction { get; set; } = HandlePureMessagePacket;
            private static void HandlePureMessagePacket(string packet)
            {
                var pureMessagePacket = Deserialize<PureMessagePacket>(packet);
                if (pureMessagePacket == null) { return; }
                else
                {
                    DebugMessage($"Received message from peer: {pureMessagePacket.Message}");
                }
            } // default behavior
        }
    }

}
