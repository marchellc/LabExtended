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
        [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));
            }
            catch (Exception ex)
            {
                ApiLog.Error("Extended API", $"An error occurred while handling a player join!\n{ex.ToColoredString()}");
            }
        }
    }
}