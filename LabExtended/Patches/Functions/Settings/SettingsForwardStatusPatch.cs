using HarmonyLib;

using LabExtended.API.Settings;

using Mirror;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    static class SettingsForwardStatusPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientStatusMsg))]
        private static bool Prefix(NetworkConnection conn, ref SSSUserStatusReport msg)
        {
            SettingsManager.OnStatusMessage(conn, msg);
            return true;
        }
    }
}