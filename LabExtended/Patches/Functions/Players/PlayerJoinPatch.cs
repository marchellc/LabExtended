using CentralAuth;

using HarmonyLib;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;

using Mirror;

using NetworkManagerUtils;

namespace LabExtended.Patches.Functions.Players
{
    public static class PlayerJoinPatch
    {
        [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.FinalizeAuthentication))]
        public static void Postfix(PlayerAuthenticationManager __instance)
        {
            try
            {
                if (!ExPlayer.TryGet(__instance._hub, out _))
                {
                    InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Extended API", $"An error occurred while handling a player join!\n{ex.ToColoredString()}");
            }
        }

        [HarmonyPatch(typeof(PlayerAuthenticationManager), nameof(PlayerAuthenticationManager.Start))]
        public static void Prefix(PlayerAuthenticationManager __instance)
        {
            if (__instance.isLocalPlayer)
                NetworkServer.ReplaceHandler(new Action<NetworkConnectionToClient, AuthenticationResponse>(PlayerAuthenticationManager.ServerReceiveAuthenticationResponse), true);
            
            if (__instance.connectionToClient is DummyNetworkConnection)
            {
                __instance.UserId = "ID_Dummy";
                
                InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));
                return;
            }
            
            if (__instance.isLocalPlayer && ServerStatic.IsDedicated)
            {
                __instance.UserId = "ID_Dedicated";
                
                InternalEvents.InternalHandlePlayerJoin(new ExPlayer(__instance._hub));
                return;
            }
            
            if (__instance.isLocalPlayer)
            {
                __instance.UserId = "ID_Host";
                
                if (PlayerAuthenticationManager.OnlineMode)
                    __instance.RequestAuthentication();
            }
            else if (!PlayerAuthenticationManager.OnlineMode)
            {
                __instance.UserId = "ID_Offline_" + __instance.netId + "_" + DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }
    }
}