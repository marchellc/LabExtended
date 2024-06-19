using CentralAuth;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions
{

    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
    public static class PlayerJoinPatch
    {
        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return;

                var player = new ExPlayer(__instance._hub);

                ExPlayer._players.Add(player);
                ExLoader.Info("Extended API", $"Player &3{player.Name}&r (&3{player.UserId}&r) &2joined&r from &3{player.Address}&r!");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"An error occured while handling a player join!\n{ex.ToColoredString()}");
            }
        }
    }
}