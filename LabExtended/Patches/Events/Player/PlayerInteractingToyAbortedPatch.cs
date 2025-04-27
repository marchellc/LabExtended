using AdminToys;

using LabExtended.Utilities;

using HarmonyLib;

using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Events;

namespace LabExtended.Patches.Events.Player;

public static class PlayerInteractingToyAbortedPatch
{
    public static FastEvent<Action<ReferenceHub>> OnSearchAborted { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub>>(typeof(InvisibleInteractableToy),
            nameof(InvisibleInteractableToy.OnSearchAborted));
    
    [HarmonyPatch(typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.ServerHandleAbort))]
    public static bool Prefix(InvisibleInteractableToy __instance, ReferenceHub hub)
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