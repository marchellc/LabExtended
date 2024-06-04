using Common.Extensions;

using System.Reflection;

namespace LabExtended.Extensions
{
    /// <summary>
    /// A class that holds extensions that target <see cref="MethodBase"/> and <see cref="MethodInfo"/> classess.
    /// </summary>
    public static class MethodExtensions
    {
        /// <summary>
        /// Returns a value indicating whether or not a specific method contains references to Unity Engine's methods.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns>A value indicating whether or not a specific method contains references to Unity Engine's methods.</returns>
        public static bool ContainsUnityReferences(this MethodBase method)
        {
            var calls = method.GetMethodCalls();

            foreach (var call in calls)
            {
                if (call is MethodInfo methodCall)
                {
                    if (methodCall.ReturnType.IsUnityType())
                        return true;

                    if (methodCall.DeclaringType.IsUnityType())
                        return true;
                }
            }

            return false;
        }
    }
}