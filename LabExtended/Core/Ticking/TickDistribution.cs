using LabExtended.API.Collections.Locked;

using LabExtended.Core.Ticking.Distributors.Unity;

using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Internals;
using LabExtended.Core.Ticking.Invokers;

using LabExtended.Core.Ticking.States;

using LabExtended.Utilities.Generation;

namespace LabExtended.Core.Ticking
{
    public static class TickDistribution
    {
        private static volatile UniqueInt32Generator _handleId = new UniqueInt32Generator(1, int.MaxValue);
        private static volatile LockedDictionary<Type, ITickDistributor> _typeToDistributor = new LockedDictionary<Type, ITickDistributor>();

        public static UnityTickDistributor UnityTick { get; } = new UnityTickDistributor();

        public static IEnumerable<ITickDistributor> AllDistributors => _typeToDistributor.Values;

        public static ITickDistributor GetDistributor(Type distributorType)
            => _typeToDistributor.TryGetValue(distributorType, out var distributor) ? distributor : throw new Exception($"Invalid distributor type: {distributorType.FullName}");

        public static object CreateWith<T>(Action<T> target, Func<T> dynamicState, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (dynamicState is null)
                throw new ArgumentNullException(nameof(dynamicState));

            return CreateWith(new TickStateInvoker<T>(new TickDynamicState<T>(dynamicState), target), options, timer);
        }

        public static object CreateWith<T>(Action<T> target, T staticState, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (staticState is null)
                throw new ArgumentNullException(nameof(staticState));

            return CreateWith(new TickStateInvoker<T>(new TickStaticState<T>(staticState), target), options, timer);
        }

        public static object CreateWith<T>(Action<T> target, ITickState<T> state, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (state is null)
                throw new ArgumentNullException(nameof(state));

            return CreateWith(new TickStateInvoker<T>(state, target), options, timer);
        }

        public static object CreateWith(Action target, ITickOptions options = null, ITickTimer timer = null)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            return CreateWith(new TickInvoker() { Target = target }, options, timer);
        }

        public static object CreateWith(ITickInvoker invoker, ITickOptions options = null, ITickTimer timer = null)
        {
            if (invoker is null)
                throw new ArgumentNullException(nameof(invoker));

            return CreateHandle(invoker, options, timer);
        }

        internal static void InternalAddDistributor(ITickDistributor distributor)
        {
            if (distributor is null)
                throw new ArgumentNullException(nameof(distributor));

            var type = distributor.GetType();

            if (_typeToDistributor.ContainsKey(type))
                throw new Exception($"This type of distributor has already been added");

            _typeToDistributor[type] = distributor;
        }

        internal static InternalTickHandle CreateHandle(ITickInvoker invoker, ITickOptions options, ITickTimer timer)
        {
            var id = InternalGetId();

            options ??= new TickOptions();

            return new InternalTickHandle
            {
                Id = id,

                Invoker = invoker,
                Options = options,
                Timer = timer,

                Paused = false
            };
        }

        internal static void InternalDestroyHandle(InternalTickHandle internalTickHandle)
        {
            if (internalTickHandle.Id > 0)
                InternalFreeId(internalTickHandle.Id);
        }

        internal static void InternalFreeId(int id)
            => _handleId.Free(id);

        internal static int InternalGetId()
            => _handleId.Next();
    }
}