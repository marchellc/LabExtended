using LabExtended.Core;

using NorthwoodLib.Pools;

using System.Reflection;

namespace LabExtended.Extensions
{
    /// <summary>
    /// Extensions targeting <see cref="Assembly"/>.
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Invokes all static methods in the specified assembly that match the given predicate, passing the provided
        /// arguments to each method.
        /// </summary>
        /// <remarks>Only static methods declared directly on each type in the assembly are considered. If
        /// a method throws an exception during invocation, the exception is logged and the process continues with the
        /// next method. This method does not throw exceptions for individual method invocation failures.</remarks>
        /// <param name="assembly">The assembly whose static methods are to be invoked. Cannot be null.</param>
        /// <param name="predicate">A function that determines whether a static method should be invoked. The method is invoked if this function
        /// returns <see langword="true"/> for the given <see cref="MethodInfo"/>.</param>
        /// <param name="args">An array of arguments to pass to each invoked static method. The arguments must match the parameters of the
        /// target methods.</param>
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

        /// <summary>
        /// Invokes all static methods in the specified assembly that match the given predicate, in an order determined
        /// by the provided priority selector.
        /// </summary>
        /// <remarks>If a method invocation throws an exception, the exception is logged and the
        /// invocation continues with the next method. Only static methods declared directly on each type are
        /// considered; inherited static methods are not included.</remarks>
        /// <param name="assembly">The assembly whose static methods are to be discovered and invoked.</param>
        /// <param name="predicate">A function that determines whether a static method should be invoked. The method is invoked only if this
        /// predicate returns <see langword="true"/>.</param>
        /// <param name="prioritySelector">A function that assigns a priority value to each method. Methods are invoked in order based on this value.</param>
        /// <param name="isDescending">A value indicating whether methods should be invoked in descending order of priority. If <see
        /// langword="true"/>, methods with higher priority values are invoked first.</param>
        /// <param name="args">An array of arguments to pass to each static method when invoking it. The arguments must match the
        /// parameters of the methods being invoked.</param>
        public static void InvokeStaticMethods(this Assembly assembly, Func<MethodInfo, bool> predicate, Func<MethodInfo, int> prioritySelector, bool isDescending, params object[] args)
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
