using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.Events;

namespace LabExtended.Patches.Functions.Players;

public static class RegularPlayerPatch {
    [HarmonyPatch(typeof(PlayerEvents), nameof(PlayerEvents.OnJoined))]
    public static void OnJoinedPostfix(PlayerJoinedEventArgs ev) {
        if (ev.Player is ExPlayer exPlayer)
            InternalEvents.HandlePlayerJoin(exPlayer);
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Get), [typeof(ReferenceHub)])]
    public static bool PlayerGetPrefix(ReferenceHub? referenceHub, ref Player? __result) {
        if (referenceHub == null) {
            __result = null;
            return false;
        }
        __result = Player.Dictionary.TryGetValue(referenceHub, out Player player) ? player : new ExPlayer(referenceHub);
        return false;
    }

    [HarmonyPatch(typeof(Player), nameof(Player.AddPlayer), [typeof(ReferenceHub)])]
    public static bool AddPlayerPrefix() => false;

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