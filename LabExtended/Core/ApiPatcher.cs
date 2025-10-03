using HarmonyLib;

using LabExtended.Extensions;

using System.Diagnostics;
using System.Reflection;

#pragma warning disable CS8603 // Possible null reference return.

namespace LabExtended.Core;

/// <summary>
/// Used to apply patches in assemblies.
/// </summary>
public static class ApiPatcher
{
    internal static int labExPatchCountOffset = 0;

    /// <summary>
    /// Whether or not transpilers should show debug lines.
    /// </summary>
    public static bool TranspilerDebug { get; set; }

    /// <summary>
    /// Gets the patcher's Harmony instance.
    /// </summary>
    public static Harmony Harmony { get; } = new($"labextended.patcher.{DateTime.Now.Ticks}");

    /// <summary>
    /// Gets the patcher's stopwatch.
    /// </summary>
    public static Stopwatch Stopwatch { get; } = new();

    /// <summary>
    /// Gets the list of applied patches per assembly.
    /// </summary>
    public static Dictionary<Assembly, List<Tuple<MethodInfo, MethodBase>>> AssemblyPatches { get; } = new();
    
    /// <summary>
    /// Gets a list of all applied patches.
    /// </summary>
    public static Dictionary<MethodInfo, MethodBase> Patches { get; } = new();

    /// <summary>
    /// Applies all patches within an assembly.
    /// </summary>
    /// <param name="assembly">The target assembly.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void ApplyPatches(this Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        try
        {
            Stopwatch.Restart();

            var types = assembly.GetTypes();
            var config = ApiLoader.ApiConfig?.PatchSection;
            var patches = new List<Tuple<MethodInfo, MethodBase>>();

            foreach (var type in types)
            {
                foreach (var method in type.GetAllMethods())
                {
                    if (!method.IsStatic || method.DeclaringType is null || string.IsNullOrWhiteSpace(method.Name))
                        continue;

                    if (!method.HasAttribute<HarmonyPatch>(out var harmonyPatch))
                        continue;

                    if (config != null && config.DisabledPatches.Contains($"{method.DeclaringType.Name}.*"))
                        continue;

                    if (config != null && config.DisabledPatches.Contains($"{method.DeclaringType.Name}.{method.Name}"))
                        continue;

                    var isPrefix = method.HasAttribute<HarmonyPrefix>() || method.Name.Contains("Prefix");
                    var isPostfix = method.HasAttribute<HarmonyPostfix>() || method.Name.Contains("Postfix");
                    var isFinalizer = method.HasAttribute<HarmonyFinalizer>() || method.Name.Contains("Finalizer");
                    var isTranspiler = method.HasAttribute<HarmonyTranspiler>() || method.Name.Contains("Transpiler");

                    if (!isPrefix && !isPostfix && !isFinalizer && !isTranspiler)
                    {
                        ApiLog.Warn("API Patcher",
                            $"Invalid patch method: &1{method.DeclaringType.Name}.{method.Name}&r");
                        continue;
                    }

                    try
                    {
                        var targetMethod = GetTargetMethod(harmonyPatch.info);

                        if (targetMethod is null)
                        {
                            ApiLog.Warn("API Patcher",
                                $"Could not find target method of patch &1{method.GetMemberName()}&r");
                            continue;
                        }

                        var patchMethod = Harmony.Patch(targetMethod,
                            isPrefix ? new HarmonyMethod(method) : null,
                            isPostfix ? new HarmonyMethod(method) : null,
                            isTranspiler ? new HarmonyMethod(method) : null,
                            isFinalizer ? new HarmonyMethod(method) : null);

                        if (patchMethod != null)
                        {
                            patches.Add(new Tuple<MethodInfo, MethodBase>(patchMethod, targetMethod));
                            
                            Patches.Add(patchMethod, targetMethod);
                        }
                        else
                        {
                            ApiLog.Error("API Patcher",
                                $"Failed to patch method &1{targetMethod?.Name ?? "null"}&r with &1{method.DeclaringType.Name}.{method.Name}&r!");
                        }
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("API Patcher",
                            $"Failed while attempting to apply patch &1{method.DeclaringType.Name}.{method.Name}&r:\n{ex.ToColoredString()}");
                    }
                }
            }

            Stopwatch.Stop();

            if (AssemblyPatches.TryGetValue(assembly, out var assemblyPatches))
                assemblyPatches.AddRange(patches);
            else
                AssemblyPatches[assembly] = patches;

            var totalCount = patches.Count;

            if (assembly == ApiLoader.Assembly)
                totalCount += labExPatchCountOffset;

            ApiLog.Info("API Patcher",
                $"&6[&r&2{assembly.GetName().Name}&r&6]&r Applied &1{totalCount}&r patches in &3{Stopwatch.Elapsed}&r!");
        }
        catch (Exception ex)
        {
            ApiLog.Error("API Patcher", $"Patching failed!\n{ex.ToColoredString()}");
        }
    }

    /// <summary>
    /// Removes all patches within an assembly.
    /// </summary>
    /// <param name="assembly">The target assembly.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RemovePatches(this Assembly assembly)
    {
        if (assembly is null)
            throw new ArgumentNullException(nameof(assembly));

        if (AssemblyPatches.TryGetValue(assembly, out var patches))
        {
            foreach (var patch in patches)
            {
                Harmony.Unpatch(patch.Item2, patch.Item1);
            }
        }

        AssemblyPatches.Remove(assembly);
    }

    /// <summary>
    /// Removes an applied patch.
    /// </summary>
    /// <param name="patchMethod">The patch method generated by Harmony.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void RemovePatch(MethodInfo patchMethod)
    {
        if (patchMethod is null)
            throw new ArgumentNullException(nameof(patchMethod));

        if (!Patches.TryGetValue(patchMethod, out var originalMethod))
            throw new Exception($"Method '{patchMethod.GetMemberName()}' is not an active patch.");

        Harmony.Unpatch(originalMethod, patchMethod);
        
        Patches.Remove(patchMethod);
    }

    private static MethodBase GetTargetMethod(HarmonyMethod method)
    {
        if (method is null)
            return null;

        if (method.declaringType is null)
            return null;

        if (string.IsNullOrWhiteSpace(method.methodName) && (!method.methodType.HasValue ||
                                                             (method.methodType.Value != MethodType.Constructor &&
                                                              method.methodType.Value != MethodType.StaticConstructor)))
            return null;

        if (method.methodType.HasValue && method.methodType.Value != MethodType.Normal)
        {
            switch (method.methodType.Value)
            {
                case MethodType.Constructor:
                case MethodType.StaticConstructor:
                    return AccessTools.Constructor(method.declaringType, method.argumentTypes,
                        method.methodType.Value is MethodType.StaticConstructor);

                case MethodType.Setter:
                    return AccessTools.PropertySetter(method.declaringType, method.methodName);

                case MethodType.Getter:
                    return AccessTools.PropertyGetter(method.declaringType, method.methodName);

                case MethodType.Enumerator:
                    return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(method.declaringType,
                        method.methodName, method.argumentTypes));

                case MethodType.Async:
                    return AccessTools.AsyncMoveNext(AccessTools.DeclaredMethod(method.declaringType, method.methodName,
                        method.argumentTypes));
            }
        }

        return AccessTools.Method(method.declaringType, method.methodName, method.argumentTypes);
    }
}