using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Hooking.Binders
{
    public class EmptyOverloadBinder : IHookBinder
    {
        private static readonly object[] _emptyArgs = Array.Empty<object>();

        public bool BindArgs(object eventObject, out object[] args)
        {
            args = _emptyArgs;
            return true;
        }

        public bool UnbindArgs(object[] _)
            => true;
    }
}