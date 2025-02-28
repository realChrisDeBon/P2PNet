using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.Routines
{
    public interface IRoutine
    {
        string RoutineName { get; init; }
        int RoutineInterval { get; set; }
        private static void RoutineFunction(object sender, System.Timers.ElapsedEventArgs e) { }
        void SetRoutineInterval(int interval);
        void StartRoutine();
        void StopRoutine();
    }
}
