using HarmonyLib;

using LabExtended.API.Settings;

using Mirror;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    /// <summary>
    /// Provides a Harmony patch for handling server-specific settings status messages.
    /// </summary>
    public static class SettingsHandleStatusPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientStatusMsg))]
        private static bool Prefix(NetworkConnection conn, ref SSSUserStatusReport msg)
        {
            SettingsManager.OnStatusMessage(conn, msg);
            return true;
        }
    }
}