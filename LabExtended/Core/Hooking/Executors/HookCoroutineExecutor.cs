using Common.Extensions;

using MEC;

namespace LabExtended.Core.Hooking.Executors
{
    public class HookCoroutineExecutor : HookExecutor
    {
        public static readonly HookCoroutineExecutor Instance = new HookCoroutineExecutor();

        public override HookType Type { get; } = HookType.Coroutine;

        public override void Execute(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
        {
            base.Execute(hookRuntimeInfo, hookInfo, binder, resultCallback);

            var args = binder.BindArgs(hookRuntimeInfo, hookInfo);
            var returnValue = hookInfo.Target.Call(hookInfo.Instance, args);

            binder.UnbindArgs();

            if (returnValue is null)
            {
                resultCallback(new HookResult(null, HookResultType.Success));
                return;
            }

            if (returnValue is CoroutineHandle coroutineHandle)
                ExecuteHandle(coroutineHandle, hookRuntimeInfo, hookInfo, binder, resultCallback);
            else if (returnValue is IEnumerator<float> coroutineFloat)
                ExecuteMecRoutine(coroutineFloat, hookRuntimeInfo, hookInfo, binder, resultCallback);
            else
                resultCallback(new HookResult($"Unknown return type: {returnValue.GetType().FullName}", HookResultType.Error));
        }

        private static void ExecuteHandle(CoroutineHandle coroutineHandle, HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
        {
            var start = DateTime.Now;
            var maxTime = DateTime.Now;
            var timeout = ExLoader.Loader.Config.Hooks.CoroutineTimeout;
            var canKill = hookInfo.SyncOptions != null ? !hookInfo.SyncOptions.DoNotKill : true;

            IEnumerator<float> AwaitHandle()
            {
                while (Timing.IsRunning(coroutineHandle))
                {
                    yield return Timing.WaitForOneFrame;

                    if (DateTime.Now >= maxTime)
                    {
                        if (canKill)
                            Timing.KillCoroutines(coroutineHandle);

                        resultCallback(new HookResult(null, HookResultType.TimedOut));
                        yield break;
                    }
                }

                resultCallback(new HookResult(null, HookResultType.Success));
                yield break;
            }

            if (hookRuntimeInfo.Event is HookWrapper || (hookInfo.SyncOptions != null && hookInfo.SyncOptions.DoNotWait))
            {
                resultCallback(new HookResult(null, HookResultType.Success));
                return;
            }

            if (hookInfo.SyncOptions != null && hookInfo.SyncOptions.Timeout.HasValue && hookInfo.SyncOptions.Timeout.Value > 0f)
                timeout = hookInfo.SyncOptions.Timeout.Value;

            maxTime = start.AddMilliseconds(timeout);
            Timing.RunCoroutine(AwaitHandle());
        }

        private static void ExecuteMecRoutine(IEnumerator<float> coroutine, HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
            => ExecuteHandle(Timing.RunCoroutine(coroutine), hookRuntimeInfo, hookInfo, binder, resultCallback);
    }
}