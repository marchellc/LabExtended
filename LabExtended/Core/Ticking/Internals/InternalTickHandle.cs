using LabExtended.Core.Ticking.Interfaces;

using System.Diagnostics;

namespace LabExtended.Core.Ticking.Internals
{
    public class InternalTickHandle
    {
        internal volatile Stopwatch Watch;

        public volatile int Id;
        public volatile bool Paused;

        public volatile ITickInvoker Invoker;
        public volatile ITickOptions Options;
        public volatile ITickTimer Timer;

        public volatile float TickTime = 0f;

        public volatile float MaxTickTime = -1f;
        public volatile float MinTickTime = -1f;

        public override string ToString()
        {
            var str = $"InternalHandle Id={Id} Paused={Paused}";

            if (Watch != null && Watch.IsRunning)
                str += $", Timing=(Last={TickTime} ms, Max={MaxTickTime} ms, Min={MinTickTime} ms)";

            if (Timer != null)
                str += $", Timer={Timer}";

            if (Invoker != null)
                str += $", Invoker={Invoker}";

            if (Options != null)
                str += $", Options={Options}";

            return str;
        }
    }
}
