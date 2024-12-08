using CentralAuth;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Players
{
    public static class PlayerJoinPatch
    {
        public static event Action<ExPlayer> OnJoined;

        [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return;

                var player = new ExPlayer(__instance._hub);

                InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));

                OnJoined.InvokeSafe(player);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Extended API", $"An error occurred while handling a player join!\n{ex.ToColoredString()}");
            }
        }
    }
}