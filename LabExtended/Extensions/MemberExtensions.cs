using System.Reflection;

namespace LabExtended.Extensions
{
    public static class MemberExtensions
    {
        public static string GetMemberName(this MemberInfo member, bool includeDeclaringType = true, char separator = '.')
        {
            if (includeDeclaringType && member.DeclaringType != null)
                return $"{member.DeclaringType.FullName}{separator}{member.Name}";

            return member.Name;
        }

        public static bool HasAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute
            => member.GetCustomAttribute<T>(inherit) != null;

        public static bool HasAttribute<T>(this MemberInfo member, out T attribute) where T : Attribute
            => (attribute = member.GetCustomAttribute<T>()) != null;

        public static bool HasAttribute<T>(this MemberInfo member, bool inherit, out T attribute) where T : Attribute
            => (attribute = member.GetCustomAttribute<T>(inherit)) != null;
    }
}
