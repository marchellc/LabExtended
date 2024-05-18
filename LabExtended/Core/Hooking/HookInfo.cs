using Common.Values;

using System.Reflection;

namespace LabExtended.Core.Hooking
{
    public class HookInfo
    {
        public StatisticValue<double> Timing { get; }

        public HookExecutor Executor { get; }
        public HookBinder Binder { get; }
        public MethodInfo Target { get; }

        public HookPriority Priority { get; }

        public HookSyncOptionsValue SyncOptions { get; }

        public object Instance { get; }

        public long Errors { get; private set; } = 0;
        public long Total { get; private set; } = 0;

        public HookInfo(HookExecutor executor, HookBinder binder, HookPriority priority, HookSyncOptionsValue syncOptions, MethodInfo targetMethod, object targetInstance = null)
        {
            if (executor is null)
                throw new ArgumentNullException(nameof(executor));

            if (targetMethod is null)
                throw new ArgumentNullException(nameof(targetMethod));

            Executor = executor;
            Binder = binder;
            Target = targetMethod;
            Instance = targetInstance;
            Priority = priority;
            SyncOptions = syncOptions;

            Timing = new StatisticValue<double>((min, max) => (min + max) / 2, (value, max) => value > max, (value, min) => value < min);
        }

        public void Execute(HookRuntimeInfo hookRuntimeInfo, Action<HookResult> callbackResult)
        {
            var start = DateTime.Now;

            Executor.Execute(hookRuntimeInfo, this, Binder, hookResult =>
            {
                var end = DateTime.Now;
                var time = end - start;

                if (hookResult.Type != HookResultType.Success)
                    Errors++;

                Total++;
                Timing.Value = time.TotalMilliseconds;

                callbackResult(hookResult);
            });
        }
    }
}