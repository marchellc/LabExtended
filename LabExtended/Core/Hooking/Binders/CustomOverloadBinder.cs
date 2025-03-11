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

        internal CustomOverloadBinder(ParameterInfo[] parameters, PropertyInfo[] binding)
        {
            _parameters = parameters;
            _binding = binding;

            _cache = new object[parameters.Length];
            _hasCacheReturned = true;
        }

        public bool BindArgs(object eventObject, out object[] args)
        {
            if (_hasCacheReturned)
            {
                args = _cache;
                _hasCacheReturned = false;
            }
            else
            {
                args = new object[Size];
            }

            for (int i = 0; i < Size; i++)
                args[i] = _binding[i].GetValue(eventObject);

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