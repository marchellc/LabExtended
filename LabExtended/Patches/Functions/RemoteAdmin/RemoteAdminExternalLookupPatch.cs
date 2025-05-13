using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;

using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.RemoteAdmin.Enums;

using MEC;

using RemoteAdmin;

namespace LabExtended.Patches.Functions.RemoteAdmin
{
    public static class RemoteAdminExternalLookupPatch
    {
        [HarmonyPatch(typeof(ExternalLookupCommand), nameof(ExternalLookupCommand.Execute))]
        public static bool Prefix(ExternalLookupCommand __instance, ArraySegment<string> arguments, ICommandSender sender, out string response, ref bool __result)
        {
            if (!RemoteAdminController.Buttons.TryGetValue(RemoteAdminButtonType.ExternalLookup, out var button) 
                || !ExPlayer.TryGet(sender, out var player))
            {
                response = null;
                return true;
            }

            if (!sender.CheckPermission(PlayerPermissions.BanningUpToDay | PlayerPermissions.LongTermBanning 
                                                                         | PlayerPermissions.SetGroup | PlayerPermissions.PlayersManagement 
                                                                         | PlayerPermissions.PermissionsManagement | PlayerPermissions.ViewHiddenBadges 
                                                                         | PlayerPermissions.PlayerSensitiveDataAccess 
                                                                         | PlayerPermissions.ViewHiddenGlobalBadges, out response))
                return __result = false;

            var playerCommandSender = sender as PlayerCommandSender;

            if (playerCommandSender == null)
            {
                response = "This command can only be executed by players!";
                return __result = false;
            }

            var text = string.Empty;

            if (arguments.Count >= 1)
            {
                if (!int.TryParse(arguments.At(0), out var playerId))
                {
                    response = "Invalid ID!";
                    return __result = false;
                }

                if (ReferenceHub.TryGetHub(playerId, out var referenceHub))
                {
                    text = referenceHub.authManager.UserId;

                    var remoteAdminExternalPlayerLookupMode = ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupMode;

                    if (remoteAdminExternalPlayerLookupMode == "fullauth")
                    {
                        Timing.RunCoroutine(__instance.AuthenticateWithExternalServer(playerCommandSender, text));

                        response = "Initiated communication with external server.";

                        __result = true;
                        return false;
                    }

                    if (remoteAdminExternalPlayerLookupMode != "urlonly")
                    {
                        response = "Invalid mode or command disabled via config.";
                        return __result = false;
                    }

                    playerCommandSender.RaReply("%" + text + "%" + ServerConfigSynchronizer.Singleton.RemoteAdminExternalPlayerLookupURL, true, false, "");
                    response = "Lookup success!";

                    __result = true;
                    return false;
                }
                else
                {
                    if (button.OnPressed(player, new int[] { playerId }))
                    {
                        response = "OK!";

                        __result = true;
                        return false;
                    }

                    response = "Invalid ID!";
                    return __result = false;
                }
            }

            response = "Unknown error";
            return __result = false;
        }
    }
}