namespace LabExtended.Core.Hooking.Binders
{
    public class HookNoParamBinder : HookBinder
    {
        private readonly object[] _emptyArgs = Array.Empty<object>();

        public static readonly HookNoParamBinder Instance = new HookNoParamBinder();

        public override object[] BindArgs(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo)
            => _emptyArgs;

        public override void UnbindArgs()
        { }
    }
}