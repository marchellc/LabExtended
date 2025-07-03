using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Searching;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements player toggles and custom items API.
/// </summary>
public static class PickUpArmorPatch
{
    [HarmonyPatch(typeof(ArmorSearchCompletor), nameof(ArmorSearchCompletor.Complete))]
    private static bool Prefix(ArmorSearchCompletor __instance)
    {
        if (!ExPlayer.TryGet(__instance.Hub, out var player) || !player.Toggles.CanPickUpItems)
            return false;

        PlayerEvents.OnSearchedPickup(new(__instance.Hub, __instance.TargetPickup));

        var pickingUpArgs =
            new PlayerPickingUpArmorEventArgs(__instance.Hub, __instance.TargetPickup as BodyArmorPickup);

        PlayerEvents.OnPickingUpArmor(pickingUpArgs);

        var pickupBehaviour =
            CustomItemUtils.GetBehaviour<CustomItemPickupBehaviour>(__instance.TargetPickup.Info.Serial);

        if (pickupBehaviour != null)
        {
            var pickingUpItemArgs = new PlayerPickingUpItemEventArgs(player.ReferenceHub, __instance.TargetPickup);
            
            pickupBehaviour.OnPickingUp(pickingUpItemArgs);

            if (!pickingUpItemArgs.IsAllowed)
                return false;
        }
        
        if (!pickingUpArgs.IsAllowed)
            return false;

        var itemType = pickupBehaviour.GetCustomValue(pb => pb.Handler.InventoryProperties.Type,
            type => type != ItemType.None, __instance.TargetItemType);
        var item = __instance.Hub.inventory.ServerAddItem(itemType,
            ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

        if (__instance._currentArmor != null)
            __instance.Hub.inventory.ServerDropItem(__instance._currentArmor.ItemSerial);

        BodyArmorUtils.SetPlayerDirty(__instance.Hub);

        var pickedUpItemArgs = new PlayerPickedUpItemEventArgs(__instance.Hub, item);
        
        pickupBehaviour.ProcessPickedUp(item, player, pickedUpItemArgs);
        
        if (__instance.TargetPickup.TryGetTracker(out var tracker))
            tracker.SetItem(item, player);
        
        if (item is BodyArmor bodyArmor)
            PlayerEvents.OnPickedUpArmor(new(__instance.Hub, bodyArmor));
        else
            PlayerEvents.OnPickedUpItem(pickedUpItemArgs);
                
        __instance.TargetPickup.DestroySelf();
        return false;
    }
}