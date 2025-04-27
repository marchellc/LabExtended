using AdminToys;
using HarmonyLib;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Toys;

using LabExtended.Events;

using LabExtended.Utilities;

using PlayerSearchingToyEventArgs = LabExtended.Events.Player.PlayerSearchingToyEventArgs;

namespace LabExtended.Patches.Events.Player;

public static class PlayerSearchingToyPatch
{
    public static FastEvent<Action<ReferenceHub>> OnSearching { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub>>
            (typeof(InvisibleInteractableToy), nameof(InvisibleInteractableToy.OnSearching));
    
    [HarmonyPatch(typeof(InvisibleInteractableToy.InteractableToySearchCompletor), nameof(InvisibleInteractableToy.InteractableToySearchCompletor.ValidateStart))]
    public static bool Prefix(InvisibleInteractableToy.InteractableToySearchCompletor __instance,
        ref bool __result)
    {
        if (!ExPlayer.TryGet(__instance.Hub, out var player))
            return false;

        if (!AdminToy.TryGet<InteractableToy>(__instance._target, out var toy))
            return false;
        
        var searchingEventArgs = new LabApi.Events.Arguments.PlayerEvents.PlayerSearchingToyEventArgs(__instance.Hub, __instance._target);
        
        PlayerEvents.OnSearchingToy(searchingEventArgs);

        if (!searchingEventArgs.IsAllowed)
            return __result = false;

        var canInteract = !__instance._target.IsLocked && __instance.ValidateDistance();
        var args = new PlayerSearchingToyEventArgs(player, toy, canInteract);

        if (!ExPlayerEvents.OnSearchingToy(args))
            return __result = false;

        OnSearching.InvokeEvent(__instance._target, __instance.Hub);
        
        __result = canInteract;
        return false;
    }
}