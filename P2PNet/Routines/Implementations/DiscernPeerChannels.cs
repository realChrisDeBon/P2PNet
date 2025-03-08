using P2PNet.Peers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Routines.Implementations
{
    public class DiscernPeerChannels : IRoutine
    {
        private readonly Timer routineTimer = new Timer();
        public int RoutineInterval { 
            get => (int)routineTimer.Interval;
            set => routineTimer.Interval = value;
        }
        public string RoutineName { get; init; }
        public DiscernPeerChannels()
        {
            RoutineName = "DiscernPeerChannels";
            routineTimer.Interval = 120000;
            routineTimer.Elapsed += new System.Timers.ElapsedEventHandler(RoutineFunction);
        }
        public void StartRoutine()
        {
            routineTimer.Start();
        }

        public void StopRoutine()
        {
            routineTimer.Stop();
        }

        public void SetRoutineInterval(int interval)
        {
            routineTimer.Interval = interval;
        }

        private static void RoutineFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            DebugMessage("Cleaning up peer channels.", ConsoleColor.Magenta);

            Task.Run(async () =>
            {
                try
                {
                    List<PeerChannel> peersToRemove = new List<PeerChannel>();

                    // LINQ statement to find channels to remove based on blocked identifiers or IPs
                    peersToRemove = PeerNetwork.ActivePeerChannels
                        .Where(channel =>
                            PeerNetwork.IncomingPeerTrustPolicy.BlockedIdentifiers.Contains(channel.peer.Identifier) ||
                            PeerNetwork.IncomingPeerTrustPolicy.BlockedIPs.Contains(channel.peer.IP))
                        .ToList();

                    // check for inactivity
                    peersToRemove.AddRange(PeerNetwork.ActivePeerChannels
                        .Where(channel => DateTime.Now - channel.LastIncomingDataReceived > TimeSpan.FromMinutes(60))
                        .ToList());

                    // remove inactive or blocked PeerChannels from ActivePeerChannels
                    foreach (PeerChannel channel in peersToRemove.Distinct())
                    {
                        bool success = await PeerNetwork.RemovePeer(channel);
                        if (success)
                        {
                            DebugMessage($"Removed peer for inactivity or being blocked: {channel.peer.IP} port {channel.peer.Port}", ConsoleColor.DarkCyan);
                            channel.ClosePeerChannel();
                        }
                    }
                }
                catch (Exception ex)
                {

                    DebugMessage($"Encountered an error: {ex}", MessageType.Critical);

                }
            });
        }

    }
}
