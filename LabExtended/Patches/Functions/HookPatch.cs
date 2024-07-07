using Common.Extensions;

using HarmonyLib;

using LabExtended.Core;
using LabExtended.Core.Hooking;

using PluginAPI.Events;

using System.Reflection;

namespace LabExtended.Patches.Functions
{
    /// <summary>
    /// A patch that replaces PluginAPI's default event execution with a call to <see cref="HookRunner.RunEvent{T}(object, T)"/>.
    /// </summary>
    public static class HookPatch
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
        public static Harmony Harmony => ExLoader.Loader.Harmony;

        /// <summary>
        /// Enables the hooking patch.
        /// </summary>
        public static void Enable()
        {
            try
            {
                _pluginApiExecuteMethod ??= typeof(EventManager).GetAllMethods().FirstOrDefault(m => m.Name == "ExecuteEvent" && m.ContainsGenericParameters);
                _replacementExecuteMethod ??= typeof(HookPatch).GetAllMethods().FirstOrDefault(m => m.Name == "RunEvent");

                foreach (var type in CancellationTypes)
                {
                    var pluginApiMethod = _pluginApiExecuteMethod.MakeGenericMethod(type);
                    var replacementMethod = _replacementExecuteMethod.MakeGenericMethod(type);

                    Harmony.Patch(pluginApiMethod, new HarmonyMethod(replacementMethod));
                };
            }
            catch (Exception ex)
            {
                ExLoader.Error("HookPatch", ex);
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