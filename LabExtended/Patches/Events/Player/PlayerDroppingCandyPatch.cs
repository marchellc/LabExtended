using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Player;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="PlayerDroppingCandyEventArgs"/> event.
/// </summary>
public static class PlayerDroppingCandyPatch
{
    [HarmonyPatch(typeof(Scp330Bag), nameof(Scp330Bag.TryRemove))]
    private static bool Prefix(Scp330Bag __instance, int index, ref CandyKindID __result)
    {
        if (!ExPlayer.TryGet(__instance.Owner, out var player))
            return true;

        if (index < 0 || index >= __instance.Candies.Count)
        {
            __result = CandyKindID.None;
            return false;
        }

        var droppingArgs = new PlayerDroppingCandyEventArgs(player, __instance, index, __instance.Candies[index]);

        if (!ExPlayerEvents.OnDroppingCandy(droppingArgs))
        {
            __result = CandyKindID.None;
            return false;
        }

        if (droppingArgs.Index < 0 || droppingArgs.Index >= __instance.Candies.Count)
        {
            __result = CandyKindID.None;
            return false;
        }

        __instance.Candies.RemoveAt(index);
        __instance.ServerRefreshBag();

        __result = droppingArgs.Type;
        return false;
    }
}
