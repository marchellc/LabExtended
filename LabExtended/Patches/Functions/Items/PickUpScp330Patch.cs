using HarmonyLib;

using InventorySystem.Items.Usables.Scp330;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Utilities;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements the functionality of the <see cref="SwitchContainer.CanPickUpItems"/> toggle.
/// </summary>
public static class PickUpScp330Patch
{
    [HarmonyPatch(typeof(Scp330SearchCompletor), nameof(Scp330SearchCompletor.Complete))]
    private static bool Prefix(Scp330SearchCompletor __instance)
    {
        if (!ExPlayer.TryGet(__instance.Hub, out var player))
            return false;
        
        if (__instance.TargetPickup is not Scp330Pickup scp330Pickup)
            return false;

        if (!player.Toggles.CanPickUpItems)
            return false;
        
        PlayerEvents.OnSearchedPickup(new(__instance.Hub, __instance.TargetPickup));

        var pickingUpArgs = new PlayerPickingUpScp330EventArgs(__instance.Hub, scp330Pickup);
        
        PlayerEvents.OnPickingUpScp330(pickingUpArgs);

        if (!pickingUpArgs.IsAllowed)
            return false;
        
        Scp330Bag.ServerProcessPickup(__instance.Hub, scp330Pickup, out var scp330Bag);

        if (scp330Pickup.StoredCandies.Count == 0)
        {
            scp330Pickup.DestroySelf();
        }
        else
        {
            var info = scp330Pickup.Info;
            
            info.InUse = false;
            
            scp330Pickup.NetworkInfo = info;
        }
        
        PlayerEvents.OnPickedUpScp330(new(__instance.Hub, scp330Pickup, scp330Bag));
        return false;
    }
}
