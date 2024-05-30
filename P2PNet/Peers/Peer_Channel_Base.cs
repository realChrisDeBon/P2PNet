using P2PNet.Distribution;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static P2PNet.Distribution.Distribution_Protocol;

namespace P2PNet.Peers
    {
    public abstract class Peer_Channel_Base
        {
        internal readonly static Dictionary<int, string> ErrorCodeDescriptions = new Dictionary<int, string>()
        {
            { 10050, "Network is down. Socket operation encountered a dead network." },
            { 10051, "Network is unreachable. Socket operation attempted to unreachable network." },
            { 10052, "Network dropped connection on reset. Connection broken due to keep-alive activity." },
            { 10053, "Software caused connection abort. Established connection aborted by software." },
            { 10054, "Connection reset by peer. Existing connection forcibly closed by remote host." },
            { 10055, "No buffer space available. Operation on socket couldn't be performed due to lack of buffer space or full queue." },
            { 10056, "Socket is already connected. Connect request made on already-connected socket." },
            { 10057, "Socket is not connected. Request to send or receive data disallowed because socket is not connected." },
            { 10058, "Cannot send after socket shutdown. Request to send or receive data disallowed due to previous socket shutdown." },
            { 10059, "Too many references. Exceeded limit of references to kernel object." },
            { 10060, "Connection timed out. Connection attempt failed due to no response from connected party." }
        }; // TODO implement more pleasant exception handling

        protected ConcurrentQueue<string> incomingData = new ConcurrentQueue<string>();
        protected ConcurrentQueue<string> outgoingData = new ConcurrentQueue<string>();
        protected ConcurrentQueue<PacketTypeRelay> packetQueue = new ConcurrentQueue<PacketTypeRelay>();

        public virtual void LoadOutgoingData(string outgoing)
            {
            outgoingData.Enqueue(outgoing);
            }

        internal Task sendTask;
        internal CancellationTokenSource cancelSender;

        internal Task receiveTask;
        internal CancellationTokenSource cancelReceiver;
        internal readonly object receiveLock = new object();

        internal Task packetHandler;
        internal CancellationTokenSource cancelPacketHandler;
        internal readonly object sendLock = new object();

        protected bool IsTrustedPeer { get; set; } = false;

        virtual public void TerminateChannel()
            {
            cancelSender.Cancel();
            cancelReceiver.Cancel();
            cancelPacketHandler.Cancel();
            }
        virtual public void TerminateCurrentSender()
            {
            cancelSender.Cancel();
            }
        virtual public void TerminateCurrentReceiver()
            {
            cancelReceiver.Cancel();
            }
        virtual public void TerminatePacketHandler()
            {
            cancelPacketHandler.Cancel();
            }
        virtual public void TrustPeer()
            {
            IsTrustedPeer = true;
            }
        virtual public void UntrustPeer()
            {
            IsTrustedPeer = false;
            }

        protected void StartPacketHandling()
            {
            cancelPacketHandler = new CancellationTokenSource();
            packetHandler = Task.Run(() => PacketHandler(cancelPacketHandler.Token));
            }
        protected async Task PacketHandler(CancellationToken cancellationToken)
            {
            while (!cancellationToken.IsCancellationRequested)
                {
                while (packetQueue.TryDequeue(out PacketTypeRelay packet))
                    {
                    switch (packet.packetType)
                        {
                        case PacketType.IdentityPacket:
                            HandleIdentityPacket(packet.Data);
                            break;
                        case PacketType.DisconnectPacket:
                            HandleDisconnectPacket(packet.Data);
                            break;
                        case PacketType.PeerGroupPacket:
                            HandlePeerGroupPacket(packet.Data);
                            break;
                        case PacketType.DataTransmissionPacket:
                            HandleDataTransmissionPacket(packet.Data);
                            break;
                        case PacketType.PureMessage:
                            HandlePureMessagePacket(packet.Data);
                            break;
                        default:
                            // BADPACKET somehow made it here
#if DEBUG
                            DebugMessage("Bad packet recieved.", MessageType.Warning);
#endif
                            break;
                        }
                    }

                Thread.Sleep(75);
                }
            }

        // Placeholders
        protected virtual void HandleIdentityPacket(string data) { DebugMessage(data, ConsoleColor.Cyan); }
        protected virtual void HandleDisconnectPacket(string data) { DebugMessage(data, ConsoleColor.Cyan); }
        protected virtual void HandlePeerGroupPacket(string data) { DebugMessage(data, ConsoleColor.Cyan); }
        protected virtual void HandleDataTransmissionPacket(string data) { DebugMessage(data, ConsoleColor.Cyan); DistributionHandler.EnqueueIncomingDataPacket(data); }
        protected virtual void HandlePureMessagePacket(string data) { DebugMessage(data, ConsoleColor.Cyan); }

        protected PacketTypeRelay ExtractWholeMessage(string receivedData)
            {
            MessageTags tags_ = new MessageTags();
            foreach (var pair in PacketTagMap)
                {
                var tags = pair.Value;
                if (receivedData.Contains(tags.OpeningTag) && receivedData.Contains(tags.ClosingTag))
                    {
                    tags_.OpeningTag = tags.OpeningTag;
                    tags_.ClosingTag = tags.ClosingTag;
                    break;
                    }
                }

            int startIndex = receivedData.IndexOf(tags_.OpeningTag); // find first opening tag
            startIndex += tags_.OpeningTag.Length;
            if (startIndex < 0)
                {
                return new PacketTypeRelay() { packetType = PacketType.BADPACKET, Data = "" }; // no opening tag found
                }

            int endIndex = receivedData.IndexOf(tags_.ClosingTag, startIndex); // Look for closing tag after opening tag

            if (endIndex < 0)
                {
                return new PacketTypeRelay() { packetType = PacketType.BADPACKET, Data = "" }; // No opening tag found
                // Not a complete message, return all data (might be partial message)
                }


            PacketTypeRelay validPacket = new PacketTypeRelay()
                {
                Data = receivedData.Replace(tags_.OpeningTag, "").Replace(tags_.ClosingTag, ""),
                packetType = PacketTagMap.FirstOrDefault(x => x.Value.OpeningTag == tags_.OpeningTag).Key
                };
            return validPacket;
            }
        protected bool IsValidMessageFormat(string inputString)
            {
            foreach (var pair in PacketTagMap)
                {
                var tags = pair.Value;
                if (inputString.Contains(tags.OpeningTag) && inputString.Contains(tags.ClosingTag))
                    {
                    return true;
                    }
                }

            return false;
            }

        }
    }
