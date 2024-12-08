using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking
{
    public static class TickExtensions
    {
        public static TickHandle CreateHandle<T>(this ITickDistributor distributor, Action<T> method, Func<T> state, ITickOptions options = null, ITickTimer timer = null)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (state is null)
                throw new ArgumentNullException(nameof(state));

            return distributor.CreateHandle(TickDistribution.CreateWith(method, state, options, timer));
        }

        public static TickHandle CreateHandle<T>(this ITickDistributor distributor, Action<T> method, T state, ITickOptions options = null, ITickTimer timer = null)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            return distributor.CreateHandle(TickDistribution.CreateWith(method, state, options, timer));
        }

        public static TickHandle CreateHandle<T>(this ITickDistributor distributor, Action<T> method, ITickState<T> state, ITickOptions options = null, ITickTimer timer = null)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            if (state is null)
                throw new ArgumentNullException(nameof(state));

            return distributor.CreateHandle(TickDistribution.CreateWith(method, state, options, timer));
        }

        public static TickHandle CreateHandle(this ITickDistributor distributor, Action method, ITickOptions options = null, ITickTimer timer = null)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            if (method is null)
                throw new ArgumentNullException(nameof(method));

            return distributor.CreateHandle(TickDistribution.CreateWith(method, options, timer));
        }

        public static TickHandle CreateHandle(this ITickDistributor distributor, ITickInvoker invoker, ITickOptions options = null, ITickTimer timer = null)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            if (invoker is null)
                throw new ArgumentNullException(nameof(invoker));

            return distributor.CreateHandle(TickDistribution.CreateWith(invoker, options, timer));
        }
    }
}
