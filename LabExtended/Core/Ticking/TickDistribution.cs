using LabExtended.API.Collections.Locked;

using LabExtended.Core.Ticking.Distributors.Timer;
using LabExtended.Core.Ticking.Distributors.Unity;

using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Internals;
using LabExtended.Core.Ticking.Invokers;
using LabExtended.Core.Ticking.States;

using LabExtended.Extensions;

using System.Diagnostics;

namespace LabExtended.Core.Ticking
{
    public static class TickDistribution
    {
        private static volatile int _idClock = 0;

        public static UnityTickDistributor UnityTick { get; } = new UnityTickDistributor();
        public static TimerTickDistributor TimerTick { get; } = new TimerTickDistributor();

        public static volatile LockedHashSet<Tuple<Type, ITickDistributor>> Distributors;

        public static ITickDistributor GetDistributor(string distributorName)
            => Distributors.TryGetFirst(x => x.Item1.Name == distributorName || x.Item1.FullName == distributorName, out var pair) ? pair.Item2 : throw new Exception($"Invalid distributor name: {distributorName}");

        public static ITickDistributor GetDistributor(Type distributorType)
            => Distributors.TryGetFirst(x => x.Item1 == distributorType, out var distributor) ? distributor.Item2 : throw new Exception($"Invalid distributor type: {distributorType.FullName}");

        public static InternalTickHandle CreateWith<T>(Action<T> target, Func<T> dynamicState, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (dynamicState is null)
                throw new ArgumentNullException(nameof(dynamicState));

            return CreateWith(new TickStateInvoker<T>(new TickDynamicState<T>(dynamicState), target), options, timer);
        }

        public static InternalTickHandle CreateWith<T>(Action<T> target, T staticState, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (staticState is null)
                throw new ArgumentNullException(nameof(staticState));

            return CreateWith(new TickStateInvoker<T>(new TickStaticState<T>(staticState), target), options, timer);
        }

        public static InternalTickHandle CreateWith<T>(Action<T> target, ITickState<T> state, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (state is null)
                throw new ArgumentNullException(nameof(state));

            return CreateWith(new TickStateInvoker<T>(state, target), options, timer);
        }

        public static InternalTickHandle CreateWith(Action target, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return CreateWith(new TickInvoker() { Target = target }, options, timer);
        }

        public static InternalTickHandle CreateWith(ITickInvoker invoker, ITickOptions options = null, ITickTimer timer = null)
        {
            if (invoker is null)
                throw new ArgumentNullException(nameof(invoker));

            return CreateHandle(invoker, options, timer);
        }

        public static void AddDistributor(ITickDistributor distributor)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            Distributors ??= new LockedHashSet<Tuple<Type, ITickDistributor>>();

            var type = distributor.GetType();

            if (Distributors.Any(x => x.Item1 == type))
                throw new Exception($"This type of distributor has already been added");

            Distributors.Add(new Tuple<Type, ITickDistributor>(type, distributor));

            ApiLog.Info("Tick Distribution", $"Added a new tick distributor: {type.Name} ({distributor})");
        }

        public static void RemoveDistributor(ITickDistributor distributor)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            Distributors ??= new LockedHashSet<Tuple<Type, ITickDistributor>>();
            Distributors.RemoveWhere(x => x.Item1 == distributor.GetType());
        }

        public static InternalTickHandle CreateHandle(ITickInvoker invoker, ITickOptions options, ITickTimer timer)
        {
            if (invoker is null)
                throw new ArgumentNullException(nameof(invoker));

            var id = _idClock++;

            options ??= new TickOptions();

            var handle = new InternalTickHandle
            {
                Id = id,

                Invoker = invoker,
                Options = options,
                Timer = timer,

                Paused = false
            };

            if (ApiLoader.ApiConfig.TickSection.EnableMetrics)
                handle.Watch = new Stopwatch();

            return handle;
        }

        public static void DestroyHandle(InternalTickHandle internalTickHandle)
        {
            if (internalTickHandle.Watch != null)
            {
                internalTickHandle.Watch.Reset();
                internalTickHandle.Watch = null;
            }
        }
    }
}