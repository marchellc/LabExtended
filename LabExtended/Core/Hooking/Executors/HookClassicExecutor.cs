using Common.Extensions;

namespace LabExtended.Core.Hooking.Executors
{
    public class HookClassicExecutor : HookExecutor
    {
        public static readonly HookClassicExecutor Instance = new HookClassicExecutor();

        public override HookType Type { get; } = HookType.Classic;

        public override void Execute(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
        {
            var args = binder.BindArgs(hookRuntimeInfo, hookInfo);
            var result = default(HookResult);

            try
            {
                var methodResult = hookInfo.Target.Call(hookInfo.Instance, args);
                result = new HookResult(methodResult, HookResultType.Success);
            }
            catch (Exception ex)
            {
                result = new HookResult(ex, HookResultType.Error);
            }

            resultCallback(result);
            binder.UnbindArgs();
        }
    }
}