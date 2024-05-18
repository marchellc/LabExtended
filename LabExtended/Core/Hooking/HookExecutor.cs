namespace LabExtended.Core.Hooking
{
    public class HookExecutor
    {
        public virtual HookType Type => throw new NotImplementedException();

        public virtual void Execute(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo, HookBinder binder, Action<HookResult> resultCallback)
        { }
    }
}