namespace LabExtended.Core.Hooking.Binders
{
    public class HookEventParamBinder : HookBinder
    {
        private object[] _buffer;
        private bool _used;

        public HookEventParamBinder()
        {
            _used = false;
            _buffer = new object[1];
        }

        public override object[] BindArgs(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo)
        {
            if (_used)
                return new object[] { hookRuntimeInfo.Event };

            _used = true;
            _buffer[0] = hookRuntimeInfo.Event;

            return _buffer;
        }

        public override void UnbindArgs()
        {
            _buffer[0] = null;
            _used = false;
        }
    }
}