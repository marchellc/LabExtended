namespace LabExtended.Core.Hooking
{
    public class HookBinder
    {
        public virtual object[] BindArgs(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo)
            => throw new NotImplementedException();

        public virtual void UnbindArgs()
            => throw new NotImplementedException();
    }
}