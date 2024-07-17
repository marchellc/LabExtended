using LabExtended.API.Collections.Locked;

using System.Reflection;

namespace LabExtended.Extensions
{
    public static class MethodExtensions
    {
        public const BindingFlags Flags = TypeExtensions.Flags;

        private static readonly LockedDictionary<MethodBase, ParameterInfo[]> _parameters = new LockedDictionary<MethodBase, ParameterInfo[]>();

        public static ParameterInfo[] GetAllParameters(this MethodBase method)
        {
            if (_parameters.TryGetValue(method, out var parameters))
                return parameters;

            return _parameters[method] = method.GetParameters();
        }
    }
}