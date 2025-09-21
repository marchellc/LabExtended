using System.Reflection;

namespace LabExtended.Extensions;
/// <summary>
/// Extensions targeting reflection methods.
/// </summary>
public static class MethodExtensions
{
    /// <summary>
    /// Binding flags of public / private and instance / static members.
    /// </summary>
    public const BindingFlags Flags = TypeExtensions.Flags;

    private static readonly Dictionary<MethodBase, ParameterInfo[]> _parameters = new();

    /// <summary>
    /// Gets all (cached) parameters in a method's overload.
    /// </summary>
    public static ParameterInfo[] GetAllParameters(this MethodBase method)
    {
        if (_parameters.TryGetValue(method, out var parameters))
            return parameters;

        return _parameters[method] = method.GetParameters();
    }
}