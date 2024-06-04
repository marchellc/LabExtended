using Common.Extensions;

namespace LabExtended.Extensions
{
    /// <summary>
    /// A class that holds extension methods of the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Gets a value indicating whether or not a specific type belongs to the Unity Engine system.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>A value indicating whether or not a specific type belongs to the Unity Engine system.</returns>
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