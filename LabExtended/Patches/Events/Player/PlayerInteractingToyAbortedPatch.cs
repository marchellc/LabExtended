using AdminToys;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities;

using HarmonyLib;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Toys;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="PlayerInteractingToyAbortedEventArgs"/> event.
/// </summary>
public static class PlayerInteractingToyAbortedPatch
{
    private static FastEvent<Action<ReferenceHub>> OnSearchAborted { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub>>(typeof(InvisibleInteractableToy),
            nameof(InvisibleInteractableToy.OnSearchAborted));
    
    [HarmonyPatch(typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.ServerHandleAbort))]
    private static bool Prefix(InvisibleInteractableToy __instance, ReferenceHub hub)
    {
        if (!ExPlayer.TryGet(hub, out var player))
            return false;

        if (!AdminToy.TryGet<InteractableToy>(__instance, out var toy))
            return false;

        OnSearchAborted.InvokeEvent(__instance, hub);
        
        PlayerEvents.OnSearchToyAborted(new(hub, __instance));
        ExPlayerEvents.OnInteractingToyAborted(new(player, toy));
        
        return false;
    }
}