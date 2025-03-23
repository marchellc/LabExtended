using HarmonyLib;

using LabExtended.Attributes;
using LabExtended.Extensions;

using System.Diagnostics;
using System.Reflection;

namespace LabExtended.Core
{
    public static class ApiPatcher
    {
        public static bool TranspilerDebug { get; set; }
        
        public static Harmony Harmony { get; } = new($"labextended.patcher.{DateTime.Now.Ticks}");

        public static Stopwatch Stopwatch { get; } = new();

        public static Dictionary<Assembly, List<Tuple<MethodInfo, MethodBase>>> AssemblyPatches { get; } = new();
        public static Dictionary<Type, List<Tuple<MethodBase, MethodInfo>>> EventPatches { get; } = new();
        public static Dictionary<MethodInfo, MethodBase> OtherPatches { get; } = new();

        public static void ApplyPatches(Assembly assembly)
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

                        var eventPatchAttributes = method.GetCustomAttributes<EventPatchAttribute>();
                        var eventRegistered = false;
                        var eventDenied = false;

                        foreach (var eventPatch in eventPatchAttributes)
                        {
                            if (eventPatch.EventType != null)
                            {
                                if (config != null && config.DisabledPatches.Contains(eventPatch.EventType.Name))
                                {
                                    ApiLog.Warn("API Patcher", $"Event patch &1{eventPatch.EventType.Name}&r was disabled by config.");

                                    eventDenied = true;
                                    break;
                                }

                                if (ApiEvents.CountEvents(eventPatch.EventType) > 0)
                                    eventRegistered = true;
                            }
                        }

                        if (eventDenied)
                            continue;

                        if (eventPatchAttributes.Any(x => x.EventType != null && !x.IsFunctionPatch) && !eventRegistered)
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
                            ApiLog.Warn("API Patcher", $"Invalid patch method: &1{method.DeclaringType.Name}.{method.Name}&r");
                            continue;
                        }

                        try
                        {
                            var targetMethod = GetTargetMethod(harmonyPatch.info);

                            if (targetMethod is null)
                            {
                                ApiLog.Warn("API Patcher", $"Could not find target method of patch &1{method.GetMemberName()}&r");
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

                                if (eventPatchAttributes.Any())
                                {
                                    foreach (var hookPatch in eventPatchAttributes)
                                    {
                                        if (hookPatch.EventType != null)
                                        {
                                            if (EventPatches.TryGetValue(hookPatch.EventType, out var eventPatches))
                                                eventPatches.Add(new Tuple<MethodBase, MethodInfo>(targetMethod, patchMethod));
                                            else
                                                EventPatches[hookPatch.EventType] = new() { new(targetMethod, patchMethod) };
                                        }
                                    }
                                }
                                else
                                {
                                    OtherPatches.Add(patchMethod, targetMethod);
                                }
                            }
                            else
                            {
                                ApiLog.Error("API Patcher", $"Failed to patch method &1{targetMethod?.Name ?? "null"}&r with &1{method.DeclaringType.Name}.{method.Name}&r!");
                            }
                        }
                        catch (Exception ex)
                        {
                            ApiLog.Error("API Patcher", $"Failed while attempting to apply patch &1{method.DeclaringType.Name}.{method.Name}&r:\n{ex.ToColoredString()}");
                        }
                    }
                }

                Stopwatch.Stop();

                if (AssemblyPatches.TryGetValue(assembly, out var assemblyPatches))
                    assemblyPatches.AddRange(patches);
                else
                    AssemblyPatches[assembly] = patches;

                ApiLog.Info("API Patcher", $"&6[&r&2{assembly.GetName().Name}&r&6]&r Applied &1{patches.Count}&r patches in &3{Stopwatch.Elapsed}&r!");
            }
            catch (Exception ex)
            {
                ApiLog.Error("API Patcher", $"Patching failed!\n{ex.ToColoredString()}");
            }
        }

        public static void RemovePatches(Assembly assembly)
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

        public static void RemovePatch(MethodInfo patchMethod)
        {
            if (patchMethod is null)
                throw new ArgumentNullException(nameof(patchMethod));

            if (!OtherPatches.TryGetValue(patchMethod, out var originalMethod))
                throw new Exception($"Method '{patchMethod.GetMemberName()}' is not an active patch.");

            Harmony.Unpatch(originalMethod, patchMethod);
            OtherPatches.Remove(patchMethod);
        }

        public static void PatchEvent(Type eventType, MethodBase targetMethod, HarmonyMethod overrideMethod)
        {
            if (eventType is null)
                throw new ArgumentNullException(nameof(eventType));

            if (EventPatches.ContainsKey(eventType))
                UnpatchEvent(eventType);

            var patchMethod = Harmony.Patch(targetMethod, overrideMethod);

            if (patchMethod != null)
            {
                if (EventPatches.TryGetValue(eventType, out var patches))
                    patches.Add(new Tuple<MethodBase, MethodInfo>(targetMethod, patchMethod));
                else
                    EventPatches[eventType] = new List<Tuple<MethodBase, MethodInfo>>() { new Tuple<MethodBase, MethodInfo>(targetMethod, patchMethod) };
            }
        }

        public static void UnpatchEvent(Type eventType)
        {
            if (eventType is null)
                throw new ArgumentNullException(nameof(eventType));

            if (!EventPatches.TryGetValue(eventType, out var patches))
                throw new Exception($"Event '{eventType.FullName}' is not patched.");

            foreach (var patchMethod in patches)
                Harmony.Unpatch(patchMethod.Item1, patchMethod.Item2);

            EventPatches.Remove(eventType);
        }

        private static MethodBase GetTargetMethod(HarmonyMethod method)
        {
            if (method is null)
                return null;

            if (method.declaringType is null)
                return null;

            if (string.IsNullOrWhiteSpace(method.methodName))
                return null;

            if (method.methodType.HasValue && method.methodType.Value != MethodType.Normal)
            {
                switch (method.methodType.Value)
                {
                    case MethodType.Constructor:
                    case MethodType.StaticConstructor:
                        return AccessTools.Constructor(method.declaringType, null, method.methodType.Value is MethodType.StaticConstructor);

                    case MethodType.Setter:
                        return AccessTools.PropertySetter(method.declaringType, method.methodName);

                    case MethodType.Getter:
                        return AccessTools.PropertyGetter(method.declaringType, method.methodName);

                    case MethodType.Enumerator:
                        return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(method.declaringType, method.methodName, method.argumentTypes));

                    case MethodType.Async:
                        return AccessTools.AsyncMoveNext(AccessTools.DeclaredMethod(method.declaringType, method.methodName, method.argumentTypes));
                }
            }

            return AccessTools.Method(method.declaringType, method.methodName, method.argumentTypes);
        }
    }
}