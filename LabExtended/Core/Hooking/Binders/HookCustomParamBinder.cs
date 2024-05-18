using System.Reflection;

namespace LabExtended.Core.Hooking.Binders
{
    public class HookCustomParamBinder : HookBinder
    {
        private readonly ParameterInfo[] _params;
        private readonly object[] _buffer;
        private readonly int _size;

        private bool _used;

        public HookCustomParamBinder(ParameterInfo[] parameters)
        {
            _params = parameters;
            _buffer = new object[parameters.Length];
            _size = parameters.Length;
            _used = false;
        }

        public override object[] BindArgs(HookRuntimeInfo hookRuntimeInfo, HookInfo hookInfo)
        {
            var buffer = _buffer;

            if (_used)
                buffer = new object[_size];
            else
                _used = true;

            ProcessBuffer(buffer, hookRuntimeInfo);
            return buffer;
        }

        public override void UnbindArgs()
        {
            _used = false;
        }

        private void ProcessBuffer(object[] buffer, HookRuntimeInfo hookRuntimeInfo)
        {
            for (int i = 0; i < _params.Length; i++)
            {
                var paramInfo = _params[i];
                var paramObject = default(object);

                foreach (var evObject in hookRuntimeInfo.EventObjects)
                {
                    if (evObject.PropertyName.ToLower() == paramInfo.Name.ToLower()
                        && evObject.PropertyValue.GetType() == paramInfo.ParameterType)
                    {
                        paramObject = evObject.PropertyValue;
                        break;
                    }
                }

                if (paramObject is null)
                    throw new InvalidOperationException($"Unknown parameter for event! '{paramInfo.Name} of type {paramInfo.ParameterType.FullName} was not present in event'");

                buffer[i] = paramObject;
            }
        }
    }
}