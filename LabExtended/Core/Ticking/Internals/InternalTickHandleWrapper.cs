using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.Internals
{
    public class InternalTickHandleWrapper<T> where T : class, ITickOptions
    {
        public volatile InternalTickHandle Base;
        public volatile T Options;

        public InternalTickHandleWrapper(InternalTickHandle internalTickHandle, T options)
        {
            Base = internalTickHandle;
            Options = options;
        }
    }
}