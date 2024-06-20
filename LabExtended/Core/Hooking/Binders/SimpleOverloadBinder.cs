using Common.Pooling.Pools;

using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Hooking.Binders
{
    public class SimpleOverloadBinder : IHookBinder
    {
        private object[] _cache;
        private bool _hasCacheReturned;

        public SimpleOverloadBinder(bool usePooling = true)
        {
            _cache = new object[1];
            _hasCacheReturned = true;
        }

        public bool UsePooling { get; set; } = true;

        public bool BindArgs(object eventObject, out object[] args)
        {
            if (UsePooling)
            {
                args = ArrayPool<object>.Shared.Rent(1);
            }
            else if (_hasCacheReturned)
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

            if (UsePooling)
            {
                ArrayPool<object>.Shared.Return(args);
                return true;
            }

            if (!_hasCacheReturned && args == _cache)
            {
                _hasCacheReturned = true;
                return true;
            }

            return true;
        }
    }
}