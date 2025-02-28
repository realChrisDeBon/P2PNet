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
            DebugMessage("DISCERNING CHANNELS", ConsoleColor.Magenta);

            Task.Run(async () =>
            {
                try
                {
                    List<PeerChannel> peersToRemove = new List<PeerChannel>();
                    List<PeerChannel> peersToTrust = new List<PeerChannel>();

                    foreach (PeerChannel channel in PeerNetwork.ActivePeerChannels)
                    {
                        if (DateTime.Now - channel.LastIncomingDataReceived > TimeSpan.FromMinutes(60000)) // No activity for 60 seconds ~ subject to change
                        {
                            peersToRemove.Add(channel);
                        }
                        if (channel.GOODPINGS > 2)
                        {
                            peersToTrust.Add(channel);
                        }
                    }

                    // Remove inactive PeerChannels from ActivePeerChannels
                    foreach (PeerChannel channel in peersToRemove)
                    {
                        bool success = await PeerNetwork.RemovePeer(channel);
                        if (success == true)
                        {

                            DebugMessage($"Removed peer for inactivity: {channel.peer.IP.ToString()} port {channel.peer.Port}", ConsoleColor.DarkCyan);

                            channel.ClosePeerChannel();
                        }
                    }
                    foreach (PeerChannel channel in peersToTrust)
                    {
                        bool success = await PeerNetwork.ElevatePeerPermission(channel);
                        if (success == true)
                        {
                            channel.TrustPeer();
                        }

                        DebugMessage($"Trusting peer: {channel.peer.IP.ToString()} port {channel.peer.Port}", ConsoleColor.Cyan);

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
