using CentralAuth;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using Common.Extensions;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
    public static class PlayerJoinPatch
    {
        public static event Action<ExPlayer> OnJoined;

        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return;

                var player = new ExPlayer(__instance._hub);

                InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));

                OnJoined.Call(player);
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"An error occured while handling a player join!\n{ex.ToColoredString()}");
            }
        }
    }
}