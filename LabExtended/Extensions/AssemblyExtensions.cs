using LabExtended.Core;

using System.Reflection;

namespace LabExtended.Extensions
{
    public static class AssemblyExtensions
    {
        public static void InvokeStaticMethods(this Assembly assembly, Func<MethodInfo, bool> predicate, params object[] args)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetAllMethods())
                {
                    try
                    {
                        if (!method.IsStatic)
                            continue;

                        if (!predicate(method))
                            continue;

                        method.Invoke(null, args);
                    }
                    catch (Exception ex)
                    {
                        ApiLoader.Error("Extended API", $"Failed to invoke static method &3{method.GetMemberName()}&r in assembly &3{assembly.GetName().Name}&r due to an exception:\n{ex.ToColoredString()}");
                    }
                }
            }
        }
    }
}
