using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Routines.Implementations
{
    public class RotateBroadcastPort : IRoutine
    {
        private readonly Timer routineTimer = new Timer();
        public int RoutineInterval
        {
            get => (int)routineTimer.Interval;
            set => routineTimer.Interval = value;
        }
        public string RoutineName { get; init; }

        public RotateBroadcastPort()
        {
            RoutineName = "RotateBroadcastPort";
            routineTimer.Interval = 30000;
            routineTimer.Elapsed += new System.Timers.ElapsedEventHandler(RoutineFunction);
        }

        public void StartRoutine()
        {
            routineTimer.Enabled = true;
            routineTimer.Start();
        }
        public void StopRoutine()
        {
            routineTimer.Stop();
            routineTimer.Enabled = false;
        }
        public void SetRoutineInterval(int interval)
        {
            routineTimer.Interval = interval;
        }
        private static void RoutineFunction(object sender, System.Timers.ElapsedEventArgs e)
        {
            Random randomizer = new Random();
            int currentDesgPort = PeerNetwork.BroadcasterPort;
            while (PeerNetwork.BroadcasterPort == currentDesgPort) // make sure we get a new port
            {
                PeerNetwork.BroadcasterPort = PeerNetwork.DesignatedPorts[randomizer.Next(PeerNetwork.DesignatedPorts.Count)];
            }

            DebugMessage($"Rotated to new port: {PeerNetwork.BroadcasterPort}", MessageType.General);

        }

    }
}
