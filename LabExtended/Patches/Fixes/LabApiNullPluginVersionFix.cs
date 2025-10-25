using HarmonyLib;

using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

using LabExtended.Core;
using LabExtended.Extensions;

using System.Reflection;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Fixes the server hanging on startup when a plugin with a null RequiredApiVersion is present.
    /// </summary>
    public static class LabApiNullPluginVersionFix
    {
        private static MethodInfo targetMethod = typeof(PluginLoader).FindMethod(nameof(PluginLoader.ValidateVersion));
        private static MethodInfo patchMethod = typeof(LabApiNullPluginVersionFix).FindMethod(nameof(Prefix));

        private static bool Prefix(Plugin plugin, ref bool __result)
        {
            __result = true;
            return false;
        }

        internal static void Internal_Init()
        {
            try
            {
                if (ApiPatcher.Harmony.Patch(targetMethod, new HarmonyMethod(patchMethod)) != null)
                {
                    ApiPatcher.labExPatchCountOffset++;
                    ApiLog.Debug("LabExtended", $"Applied &6{nameof(LabApiNullPluginVersionFix)}&r");
                }
                else
                {
                    ApiLog.Error("LabExtended", $"Failed to apply &3{nameof(LabApiNullPluginVersionFix)}&r!");
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("LabExtended", ex);
            }
        }
    }
}