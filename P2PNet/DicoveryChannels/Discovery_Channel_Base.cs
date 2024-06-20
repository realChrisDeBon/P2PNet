using P2PNet.Distribution;
using P2PNet.NetworkPackets;
using P2PNet.Peers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static P2PNet.Distribution.Distribution_Protocol;
using static P2PNet.Distribution.DistributionHandler;
using static P2PNet.PeerNetwork;

namespace P2PNet.DicoveryChannels
    {

    internal static class BroadcastRateControl // Variabalize the broadcast rate like a sine wave
        {
        private static readonly Random random = new Random();
        private static System.Timers.Timer intervalTimer;

        private const double MIN_INTERVAL = 20000;  // 20 seconds
        private const double MAX_INTERVAL = 1000;    // 1 second
        private const double WAVE_DURATION = 4 * 60 * 1000; // 4 minutes

        internal static double currentInterval = MAX_INTERVAL;

        static BroadcastRateControl()
            {
            intervalTimer = new System.Timers.Timer(1000);  // Update interval frequently
            intervalTimer.Elapsed += UpdateInterval;
            intervalTimer.AutoReset = true;
            intervalTimer.Start();
            }

        private static void UpdateInterval(object sender, System.Timers.ElapsedEventArgs e)
            {
            // Calculate new interval based on sine wave
            double time = Environment.TickCount64 & Int32.MaxValue; // Get time reliably
            double sineValue = Math.Sin(2 * Math.PI * time / WAVE_DURATION);
            currentInterval = (MAX_INTERVAL - MIN_INTERVAL) * (sineValue + 1) / 2 + MIN_INTERVAL;
            }

        internal static int GetCurrentInterval()
            {
            int variation_ = random.Next(100, 300);
            return (int)currentInterval + variation_;
            }
        public static void DownThrottle()
            {
            currentInterval = MIN_INTERVAL;
            intervalTimer.Interval += 10000;
            }
        }

    internal abstract class Discovery_Channel_Base
        {

        public static Queue<int> dutypackets = new Queue<int>();
        internal static Random randomizer = new Random();
        private static System.Timers.Timer portRotationTimer;
        public readonly IdentifierPacket packet_ = new IdentifierPacket("IP", ListeningPort, PublicIPV4Address);
        public static CollectionSharePacket collectionpacket_;

        public static IPEndPoint listenerendpoint;
        public static IPEndPoint broadcasterendpoint;

        public readonly DateTime channelopened_ = new DateTime(); // record time created
        public static CancellationTokenSource task_canceltoken; // TODO: implement this

        public Discovery_Channel_Base()
            {
            channelopened_ = DateTime.Now; // log creation time
            collectionpacket_ = new CollectionSharePacket();

            // Initialize timer on class load
            portRotationTimer = new System.Timers.Timer();
            portRotationTimer.Elapsed += RotatePort;
            portRotationTimer.Interval = RandomizeInterval(); // Random initial interval
            portRotationTimer.AutoReset = true; // Keep the timer running
            portRotationTimer.Start();
            }

        internal void HandlePacket(string packet)
            {
            var identifierPacket = Deserialize<IdentifierPacket>(packet);
            var collectionPacket = Deserialize<CollectionSharePacket>(packet);

            if (identifierPacket != null)
                {
#if DEBUG
                DebugMessage($"Packet received: {identifierPacket.Message} from {identifierPacket.ip}" + Environment.NewLine + $"\t\t\tSecret Port: {identifierPacket.Data}");
#endif
                IPeer newPeer = new GenericPeer(IPAddress.Parse(identifierPacket.ip), identifierPacket.Data);
                PeerNetwork.AddPeer(newPeer);
                }
            else if (collectionPacket != null)
                {
#if DEBUG
                DebugMessage($"Packet received: {identifierPacket.Message}");
#endif
                PeerNetwork.ProcessPeerList(collectionPacket);
                }
            }

        public void DownThrottle()
            {
            BroadcastRateControl.DownThrottle();
            }

        private static double RandomizeInterval()
            {
            return randomizer.Next(3 * 60000, 5 * 60000); // 3-5 minutes in milliseconds
            }
        private static void RotatePort(object sender, System.Timers.ElapsedEventArgs e)
            {
            int currentDesgPort = BroadcasterPort;
            while (BroadcasterPort == currentDesgPort) // make sure we get a new port
                {
                BroadcasterPort = DesignatedPorts[randomizer.Next(DesignatedPorts.Count)];
                }
            portRotationTimer.Interval = RandomizeInterval();
#if DEBUG
            DebugMessage($"Rotated to new port: {BroadcasterPort}", MessageType.General);
#endif
            }

        public int CreateTimeVariation(int min, int max) { return randomizer.Next(min, max); }
        private string packet_serialized()
            {
            return System.Text.Json.JsonSerializer.Serialize(packet_, typeof(IdentifierPacket), new IdentifierPacketContext());
            }
        public byte[] UniqueIdentifier() { string temp_ = packet_serialized(); return Encoding.UTF8.GetBytes(temp_); }

        private string colpacket_serialized()
            {
            string c_ = System.Text.Json.JsonSerializer.Serialize(collectionpacket_);
            return System.Text.Json.JsonSerializer.Serialize(collectionpacket_, typeof(CollectionSharePacket), new CollectionSharePacketContext());
            }
        public byte[] CollectionShare() { string temp_ = colpacket_serialized(); return Encoding.UTF8.GetBytes(temp_); }

        public async Task DutyPacketGenerator()
            {
            int a = 1;
            dutypackets.Enqueue(a);
            }


        }
       
    }
