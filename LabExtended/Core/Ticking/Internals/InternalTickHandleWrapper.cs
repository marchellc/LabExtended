using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.Internals
{
    public class InternalTickHandleWrapper<T> where T : class, ITickOptions
    {
        public volatile InternalTickHandle Base;
        public volatile TickOptions BaseOptions;
        public volatile T Options;

        public InternalTickHandleWrapper(InternalTickHandle internalTickHandle, T options)
        {
            Base = internalTickHandle;
            BaseOptions = internalTickHandle.Options as TickOptions;

            Options = options;
        }

        public override string ToString()
            => $"Wrapper: Base={Base} | Options ({typeof(T).Name})={Options}";
    }
}