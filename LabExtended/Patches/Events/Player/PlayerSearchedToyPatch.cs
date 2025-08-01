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
/// Implements the <see cref="PlayerSearchedToyEventArgs"/> event.
/// </summary>
public static class PlayerSearchedToyPatch
{    
    private static FastEvent<Action<ReferenceHub>> OnSearched { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub>>
            (typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.OnSearched));
    
    [HarmonyPatch(typeof(InvisibleInteractableToy.InteractableToySearchCompletor), nameof(InvisibleInteractableToy.InteractableToySearchCompletor.Complete))]
    private static bool Prefix(InvisibleInteractableToy.InteractableToySearchCompletor __instance)
    {
        OnSearched.InvokeEvent(__instance._target, __instance.Hub);
        
        PlayerEvents.OnSearchedToy(new(__instance.Hub, __instance._target));
        ExPlayerEvents.OnSearchedToy(new(ExPlayer.Get(__instance.Hub), AdminToy.Get<InteractableToy>(__instance._target)));

        return false;
    }
}