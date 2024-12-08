using HarmonyLib;

using LabExtended.API.Settings;

using Mirror;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    public static class SettingsDisableStatusPatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientStatusMsg))]
        public static bool Prefix(NetworkConnection conn, ref SSSUserStatusReport msg)
        {
            SettingsManager.OnStatusMessage(conn, msg);
            return false;
        }
    }
}
