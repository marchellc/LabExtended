using HarmonyLib;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Events;

namespace LabExtended.Patches.Functions.Players;

public static class RegularPlayerPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.AddPlayer))]
    public static bool JoinPrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null && !referenceHub.isLocalPlayer)
        {
            _ = new ExPlayer(referenceHub);
        }

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
            
            InternalEvents.InternalHandlePlayerLeave(player);
            
            player.Dispose();
        }

        return false;
    }
}