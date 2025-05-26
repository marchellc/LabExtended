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

        var customItems = ListPool<CustomItemPickupBehaviour>.Shared.Rent();
        
        if (!pickingUpArgs.IsAllowed)
            return false;
        
        CustomItemUtils.GetPickupBehavioursNonAlloc(__instance.TargetPickup.Info.Serial, customItems);

        if (customItems.Count > 0)
        {
            var pickingUpItemArgs = new PlayerPickingUpItemEventArgs(__instance.Hub, __instance.TargetPickup);

            customItems.ForEach(ci => ci.OnPickingUp(pickingUpItemArgs));

            if (!pickingUpItemArgs.IsAllowed)
            {
                ListPool<CustomItemPickupBehaviour>.Shared.Return(customItems);
                return false;
            }
        }

        var targetItem = CustomItemUtils.SelectPickupBehaviour(customItems);

        if (__instance._currentArmor != null)
            __instance.Hub.inventory.ServerDropItem(__instance._currentArmor.ItemSerial);

        ItemBase? item = null;

        if (targetItem != null)
            item = __instance.Hub.inventory.ServerAddItem(targetItem.Handler.InventoryProperties.Type,
                ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);
        else
            item = __instance.Hub.inventory.ServerAddItem(__instance.TargetPickup.Info.ItemId,
                ItemAddReason.PickedUp, __instance.TargetPickup.Info.Serial, __instance.TargetPickup);

        BodyArmorUtils.SetPlayerDirty(__instance.Hub);

        var pickedUpItemArgs = new PlayerPickedUpItemEventArgs(__instance.Hub, item);

        CustomItemUtils.ProcessPickedUp(customItems, item, player, pickedUpItemArgs);
        
        if (__instance.TargetPickup.TryGetTracker(out var tracker))
            tracker.SetItem(item, player);
        
        if (item is BodyArmor bodyArmor)
            PlayerEvents.OnPickedUpArmor(new(__instance.Hub, bodyArmor));
        else
            PlayerEvents.OnPickedUpItem(pickedUpItemArgs);
                
        __instance.TargetPickup.DestroySelf();
        
        ListPool<CustomItemPickupBehaviour>.Shared.Return(customItems);
        return false;
    }
}