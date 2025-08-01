﻿using AdminToys;

using HarmonyLib;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Events;
using LabExtended.Events.Player;

using LabExtended.Utilities;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="PlayerInteractingToyEventArgs"/> event.
/// </summary>
public static class PlayerInteractingToyPatch
{
    private static FastEvent<Action<ReferenceHub>> OnInteracted { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub>>(typeof(InvisibleInteractableToy),
            nameof(InvisibleInteractableToy.OnInteracted));
    
    [HarmonyPatch(typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.ServerInteract))]
    private static bool Prefix(InvisibleInteractableToy __instance, ReferenceHub ply)
    {
        if (!ExPlayer.TryGet(ply, out var player))
            return false;

        if (!AdminToy.TryGet<InteractableToy>(__instance, out var toy))
            return false;

        if (!ExPlayerEvents.OnInteractingToy(new(player, toy, toy.CanInteract))
            || !toy.CanInteract)
            return false;

        OnInteracted.InvokeEvent(__instance, ply);
        
        PlayerEvents.OnInteractedToy(new(ply, __instance));
        ExPlayerEvents.OnInteractedToy(new(player, toy));
        
        return false;
    }
}