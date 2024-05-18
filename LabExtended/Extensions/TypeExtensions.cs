using Common.Extensions;

namespace LabExtended.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsUnityType(this Type type)
        {
            if (type.InheritsType<UnityEngine.Object>())
                return true;

            if (type.Assembly.FullName.Contains("Unity"))
                return true;

            if (type.Assembly.FullName.Contains("Assembly-CSharp"))
                return true;

            return false;
        }
    }
}