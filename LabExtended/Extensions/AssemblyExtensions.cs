using LabExtended.Core;

using NorthwoodLib.Pools;

using System.Reflection;

namespace LabExtended.Extensions
{
    public static class AssemblyExtensions
    {
        public static void InvokeStaticMethods(this Assembly assembly, Func<MethodInfo, bool> predicate, params object[] args)
        {
            foreach (var type in assembly.GetTypes())
            {
                foreach (var method in type.GetAllMethods().Where(x => x.DeclaringType == type))
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
                        ApiLog.Error("Extended API", $"Failed to invoke static method &3{method.GetMemberName()}&r in assembly &3{assembly.GetName().Name}&r due to an exception:\n{ex.ToColoredString()}");
                    }
                }
            }
        }

        public static void InvokeStaticMethods(this Assembly assembly, Func<MethodInfo, bool> predicate, Func<MethodInfo, ushort> prioritySelector, bool isDescending, params object[] args)
        {
            var types = assembly.GetTypes();
            var methods = ListPool<MethodInfo>.Shared.Rent();

            foreach (var type in types)
            {
                foreach (var method in type.GetAllMethods())
                {
                    if (method.DeclaringType is null || method.DeclaringType != type)
                        continue;

                    if (!method.IsStatic || !predicate(method))
                        continue;

                    methods.Add(method);
                }
            }

            var orderedMethods = (isDescending ? methods.OrderByDescending(prioritySelector) : methods.OrderBy(prioritySelector));

            foreach (var method in orderedMethods)
            {
                try
                {
                    method.Invoke(null, args);
                }
                catch (Exception ex)
                {
                    ApiLog.Error("LabExtended API", $"Failed to invoke static method &1{method.GetMemberName()}&r:\n{ex.ToColoredString()}");
                }
            }

            ListPool<MethodInfo>.Shared.Return(methods);
        }
    }
}
