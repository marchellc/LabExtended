using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.Internals
{
    public class InternalTickHandle
    {
        public volatile int Id;
        public volatile bool Paused;

        public volatile ITickInvoker Invoker;
        public volatile ITickOptions Options;
        public volatile ITickTimer Timer;
    }
}
