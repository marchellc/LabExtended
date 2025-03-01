using System.Reflection;

namespace LabExtended.Extensions
{
    public static class MethodExtensions
    {
        public const BindingFlags Flags = TypeExtensions.Flags;

        private static readonly Dictionary<MethodBase, ParameterInfo[]> _parameters = new();

        public static ParameterInfo[] GetAllParameters(this MethodBase method)
        {
            if (_parameters.TryGetValue(method, out var parameters))
                return parameters;

            return _parameters[method] = method.GetParameters();
        }
    }
}