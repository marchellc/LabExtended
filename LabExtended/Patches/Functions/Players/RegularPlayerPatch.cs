using HarmonyLib;
using Mirror;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Events;

using static LabExtended.API.Containers.SwitchContainer;

namespace LabExtended.Patches.Functions.Players;

public static class RegularPlayerPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.AddPlayer))]
    public static bool JoinPrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && !referenceHub.isLocalPlayer)
            _ = new ExPlayer(referenceHub, referenceHub.connectionToClient.GetType() != typeof(NetworkConnectionToClient) ? GetNewNpcToggles() : GetNewPlayerToggles());

        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.RemovePlayer))]
    public static bool LeavePrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && !referenceHub.isLocalPlayer && ExPlayer.TryGet(referenceHub, out var player) && player != null)
        {
            if (referenceHub.authManager.UserId != null)
                Player.UserIdCache.Remove(referenceHub.authManager.UserId);
            
            Player.Dictionary.Remove(referenceHub);
            
            InternalEvents.HandlePlayerLeave(player);
            
            player.Dispose();
        }

        return false;
    }
}