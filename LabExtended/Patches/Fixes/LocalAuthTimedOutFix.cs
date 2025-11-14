using CentralAuth;

using HarmonyLib;

using LabExtended.Core;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Provides a fix to prevent the local player from being incorrectly rejected due to authentication timeouts during
    /// local or dedicated server sessions.
    /// </summary>
    public static class LocalAuthTimedOutFix
    {
        [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.RejectAuthentication))]
        private static bool RejectPrefix(PlayerAuthenticationManager __instance)
        {
            if (__instance != null
                && (__instance.isLocalPlayer
                    || __instance.connectionToClient.address == "localhost"))
            {
                ApiLog.Debug("LabExtended", "Attempted to reject authentification of the local player!");

                __instance.UserId = "ID_Dedicated";

                __instance._hub.nicknameSync.UpdateNickname("Dedicated Server");
                __instance._hub.serverRoles.RefreshPermissions(false);

                return false;
            }

            return true;
        }
    }
}