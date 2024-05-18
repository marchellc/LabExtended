using Common.Extensions;

using System.Reflection;

namespace LabExtended.Extensions
{
    public static class MethodExtensions
    {
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