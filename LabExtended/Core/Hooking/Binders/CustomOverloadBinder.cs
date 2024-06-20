using Common.Extensions;
using Common.Pooling.Pools;

using LabExtended.Core.Hooking.Interfaces;

using System.Reflection;

namespace LabExtended.Core.Hooking.Binders
{
    public class CustomOverloadBinder : IHookBinder
    {
        private readonly ParameterInfo[] _parameters;
        private readonly PropertyInfo[] _binding;
        private readonly object[] _cache;

        private bool _hasCacheReturned;

        public int Size => _parameters.Length;

        public bool UsePooling { get; set; } = true;

        internal CustomOverloadBinder(ParameterInfo[] parameters, PropertyInfo[] binding, bool usePooling = true)
        {
            _parameters = parameters;
            _binding = binding;

            _cache = new object[parameters.Length];
            _hasCacheReturned = true;

            UsePooling = usePooling;
        }

        public bool BindArgs(object eventObject, out object[] args)
        {
            if (UsePooling)
            {
                args = ArrayPool<object>.Shared.Rent(Size);
            }
            else if (_hasCacheReturned)
            {
                args = _cache;
                _hasCacheReturned = false;
            }
            else
            {
                args = new object[Size];
            }

            for (int i = 0; i < Size; i++)
                args[i] = _binding[i].Get(eventObject);

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