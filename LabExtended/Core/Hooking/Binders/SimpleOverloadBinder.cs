using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Hooking.Binders
{
    public class SimpleOverloadBinder : IHookBinder
    {
        private object[] _cache;
        private bool _hasCacheReturned;

        public SimpleOverloadBinder()
        {
            _cache = new object[1];
        }

        public bool UsePooling { get; set; } = true;

        public bool BindArgs(object eventObject, out object[] args)
        {
            if (_hasCacheReturned)
            {
                args = _cache;
                _hasCacheReturned = false;
            }
            else
            {
                args = new object[1];
            }

            args[0] = eventObject;
            return true;
        }

        public bool UnbindArgs(object[] args)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            if (!_hasCacheReturned && args == _cache)
            {
                _hasCacheReturned = true;
                return true;
            }

            return true;
        }
    }
}