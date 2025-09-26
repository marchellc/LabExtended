using HarmonyLib;

using LabExtended.API.Settings;

using Mirror;

using UserSettings.ServerSpecific;

namespace LabExtended.Patches.Functions.Settings
{
    public static class SettingsDisableResponsePatch
    {
        [HarmonyPatch(typeof(ServerSpecificSettingsSync), nameof(ServerSpecificSettingsSync.ServerProcessClientResponseMsg))]
        public static bool Prefix(NetworkConnection conn, ref SSSClientResponse msg)
        {
            SettingsManager.OnResponseMessage(conn, msg);
            return true;
        }
    }
}
