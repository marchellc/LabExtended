using HarmonyLib;

using LabExtended.API.Settings;

using Mirror;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    /// <summary>
    /// Provides a Harmony patch for handling server-specific settings response messages from clients.
    /// </summary>
    public static class SettingsHandleResponsePatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientResponseMsg))]
        private static bool Prefix(NetworkConnection conn, ref SSSClientResponse msg)
        {
            SettingsManager.OnResponseMessage(conn, msg);
            return true;
        }
    }
}
