using System.Reflection;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting reflection members.
/// </summary>
public static class MemberExtensions
{
    /// <summary>
    /// Gets the full name of a member.
    /// </summary>
    public static string GetMemberName(this MemberInfo member, bool includeDeclaringType = true, char separator = '.')
    {
        if (includeDeclaringType && member.DeclaringType != null)
            return $"{member.DeclaringType.FullName}{separator}{member.Name}";

        return member.Name;
    }

    /// <summary>
    /// Checks if a member has an attribute.
    /// </summary>
    public static bool HasAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute
        => member.GetCustomAttribute<T>(inherit) != null;

    /// <summary>
    /// Checks if a member has an attribute.
    /// </summary>
    public static bool HasAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute
        => (attribute = member.GetCustomAttribute<T>()) != null;

    /// <summary>
    /// Checks if a member has an attribute.
    /// </summary>
    public static bool HasAttribute<T>(this MemberInfo member, bool inherit, out T attribute) where T : Attribute
        => (attribute = member.GetCustomAttribute<T>(inherit)) != null;
}