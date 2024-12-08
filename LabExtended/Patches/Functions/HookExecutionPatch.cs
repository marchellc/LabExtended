using HarmonyLib;
using LabExtended.API.Collections.Locked;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Extensions;

using PluginAPI.Events;

using System.Reflection;

namespace LabExtended.Patches.Functions
{
    /// <summary>
    /// A patch that replaces PluginAPI's default event execution with a call to <see cref="HookRunner.RunEvent{T}(object, T)"/>.
    /// </summary>
    public static class HookExecutionPatch
    {
        private static MethodInfo _pluginApiExecuteMethod;
        private static MethodInfo _replacementExecuteMethod;

        /// <summary>
        /// Gets all types implementing <see cref="IEventCancellation"/>.
        /// </summary>
        public static Type[] CancellationTypes { get; } = new Type[]
        {
            typeof(bool),

            typeof(PreauthCancellationData),
            typeof(RoundEndCancellationData),
            typeof(PlayerGetGroupCancellationData),
            typeof(PlayerPreCoinFlipCancellationData),
            typeof(RoundEndConditionsCheckCancellationData),
            typeof(PlayerCheckReservedSlotCancellationData),
        };

        /// <summary>
        /// Gets the <see cref="HarmonyLib.Harmony"/> instance.
        /// </summary>
        public static Harmony Harmony => ApiPatcher.Harmony;

        /// <summary>
        /// Gets a dictionary which contains all of the applied patches.
        /// </summary>
        public static LockedDictionary<MethodInfo, MethodInfo> Patches { get; } = new LockedDictionary<MethodInfo, MethodInfo>();

        /// <summary>
        /// Enables the hooking patch.
        /// </summary>
        [LoaderInitialize(1)]
        public static void Enable()
        {
            try
            {
                if (ApiLoader.ApiConfig?.PatchSection != null && ApiLoader.ApiConfig.PatchSection.DisabledPatches.Contains("HookPatch"))
                    return;

                _pluginApiExecuteMethod ??= typeof(EventManager).FindMethod(m => m.Name == "ExecuteEvent" && m.ContainsGenericParameters);
                _replacementExecuteMethod ??= typeof(HookExecutionPatch).FindMethod(m => m.Name == "RunEvent");

                foreach (var type in CancellationTypes)
                {
                    var pluginApiMethod = _pluginApiExecuteMethod.MakeGenericMethod(type);
                    var replacementMethod = _replacementExecuteMethod.MakeGenericMethod(type);
                    var patchMethod = Harmony.Patch(pluginApiMethod, new HarmonyMethod(replacementMethod));

                    if (patchMethod != null)
                        Patches.Add(patchMethod, pluginApiMethod);
                };
            }
            catch (Exception ex)
            {
                ApiLog.Error("HookPatch", ex);
            }
        }

        private static bool RunEvent<T>(IEventArguments args, ref T __result) where T : struct
        {
            FixDefaultValue(ref __result);

            __result = HookRunner.RunEvent(args, __result);
            return false;
        }

        // Just NW stuff ..
        private static void FixDefaultValue<T>(ref T value)
        {
            var isBool = typeof(T) == typeof(bool);
            var cancelled = false;

            T cancellation;

            if (isBool)
                cancellation = (T)(object)true;
            else
                cancellation = default;

            value = cancellation;
        }
    }
}