using HarmonyLib;

using LabApi.Features.Stores;

using Mirror;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Events;

using static LabExtended.API.Containers.SwitchContainer;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Used to replace LabAPI's <see cref="Player"/> class with our custom <see cref="ExPlayer"/> subtype.
/// </summary>
public static class PlayerConstructorPatch
{
    [HarmonyPatch(typeof(Player), nameof(Player.AddPlayer))]
    private static bool JoinPrefix(ReferenceHub referenceHub)
    {
        if (referenceHub != null)
        {
            if (!ExPlayer.TryGet(referenceHub, out var player))
                player = new ExPlayer(referenceHub,
                    referenceHub.isLocalPlayer || referenceHub.connectionToClient.GetType() != typeof(NetworkConnectionToClient)
                        ? GetNewNpcToggles()
                        : GetNewPlayerToggles());

            if (referenceHub.isLocalPlayer)
            {
                ExPlayer.host = player;
                Server.Host = player;
            }
        }

        return false;
    }
    
    [HarmonyPatch(typeof(Player), nameof(Player.RemovePlayer))]
    private static bool LeavePrefix(ReferenceHub referenceHub)
    {
        Player.Dictionary.Remove(referenceHub);
        
        if (referenceHub.authManager.UserId != null)
            Player.UserIdCache.Remove(referenceHub.authManager.UserId);
        
        if (referenceHub.isLocalPlayer)
        {
            ExPlayer.host = null;
            Server.Host = null;
        }
        
        if (ExPlayer.TryGet(referenceHub, out var player) && player != null)
        {
            CustomDataStoreManager.RemovePlayer(player);
            
            InternalEvents.HandlePlayerLeave(player);
            
            player.Dispose();
        }

        return false;
    }
}