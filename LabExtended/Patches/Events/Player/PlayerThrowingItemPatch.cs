using HarmonyLib;

using InventorySystem;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabApi.Features.Wrappers;

using LabExtended.API;
using LabExtended.API.Custom.Items;
using LabExtended.API.Custom.Items.Events;

using LabExtended.Extensions;
using LabExtended.Events;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

using PlayerThrowingItemEventArgs = LabExtended.Events.Player.PlayerThrowingItemEventArgs;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements player toggles and custom item methods.
/// </summary>
public static class PlayerThrowingItemPatch
{
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem__UInt16__Boolean))]
    private static bool Prefix(Inventory __instance, ushort itemSerial, bool tryThrow)
    {
        var player = ExPlayer.Get(__instance._hub);

        if (player is null)
            return true;

        if (!player.Toggles.CanDropItems)
            return false;

        if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var item) || !item.AllowHolster)
            return false;

        var droppingArgs = new PlayerDroppingItemEventArgs(player.ReferenceHub, item, tryThrow);

        PlayerEvents.OnDroppingItem(droppingArgs);

        if (!droppingArgs.IsAllowed)
            return false;

        tryThrow = droppingArgs.Throw;

        CustomItem.Internal_GetCustomItem(itemSerial, out var customItem, out var customItemTracker);

        if (__instance.CurInstance != null && player.Inventory.Snake.Keycard != null &&
            __instance.CurInstance == player.Inventory.Snake.Keycard)
            player.Inventory.Snake.Reset(false, true);

        if (customItem != null)
        {
            customItem.OnDroppingItem(droppingArgs, ref customItemTracker.Data);

            tryThrow = droppingArgs.Throw;

            if (!droppingArgs.IsAllowed)
                return false;
        }

        var pickupType = customItem?.PickupType ?? item.ItemTypeId;
        var pickupScale = customItem?.Scale ?? Vector3.one;

        var keepItem = false;
        var spawnPickup = pickupType != item.ItemTypeId || pickupScale != Vector3.one 
            ? pickupType != ItemType.None && !pickupType.IsAmmo()
            : item.SimulateDrop(out keepItem);

        var pickup = spawnPickup 
            ? (pickupType != item.ItemTypeId || pickupScale != Vector3.one
                    ? ExMap.SpawnItem(pickupType, player.Position, pickupScale, player.Rotation, itemSerial)
                    : player.ReferenceHub.inventory.ServerDropItem(itemSerial))
            : null;

        __instance.SendItemsNextFrame = true;
        
        var droppedArgs = new PlayerDroppedItemEventArgs(player.ReferenceHub, pickup, tryThrow);

        PlayerEvents.OnDroppedItem(droppedArgs);

        if (customItem != null)
        {
            var customItemDroppedEventArgs = new CustomItemDroppedEventArgs(customItem, player, item, pickup, customItemTracker.Data, tryThrow);

            if (player.Inventory.CurrentCustomItem != null 
                && player.Inventory.CurrentCustomItem == customItem
                && player.Inventory.CurrentItemIdentifier.SerialNumber == item.ItemSerial)
                player.Inventory.CurrentCustomItem = null;

            player.Inventory.ownedCustomItems.Remove(item.ItemSerial);

            customItem.OnItemDropped(customItemDroppedEventArgs);

            customItemTracker.Data = customItemDroppedEventArgs.PickupData;

            customItemTracker.Item = null;
            customItemTracker.Owner = null;

            customItemTracker.Pickup = pickup;
        }

        if (pickup != null)
            player.Inventory.droppedItems?.Add(pickup);          
        
        if (item != null && !keepItem)
            item.DestroyItem();

        if (pickup != null && player.Toggles.CanThrowItems && tryThrow && pickup.TryGetRigidbody(out var rigidbody))
        {
            var throwingArgs =
                new LabApi.Events.Arguments.PlayerEvents.PlayerThrowingItemEventArgs(player.ReferenceHub, pickup,
                    rigidbody);

            PlayerEvents.OnThrowingItem(throwingArgs);

            if (!throwingArgs.IsAllowed)
                return false;

            var velocity = __instance._hub.GetVelocity();
            var angular = Vector3.Lerp(item.ThrowSettings.RandomTorqueA, item.ThrowSettings.RandomTorqueB,
                UnityEngine.Random.value);

            velocity = velocity / 3f + __instance._hub.PlayerCameraReference.forward * 6f *
                (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, rigidbody.mass)) + 0.3f);

            velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
            velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
            velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

            var throwingEv = new PlayerThrowingItemEventArgs(player, droppingArgs.Item, Pickup.Get(pickup), rigidbody,
                __instance._hub.PlayerCameraReference.position, velocity, angular);

            if (!ExPlayerEvents.OnThrowingItem(throwingEv))
                return false;

            if (customItem != null)
            {
                var dataTemp = customItemTracker.Data;

                customItem.OnThrowingItem(throwingEv, ref dataTemp);
                customItemTracker.Data = dataTemp;
            }

            if (!throwingEv.IsAllowed)
                return false;

            velocity = throwingEv.Velocity;
            angular = throwingEv.AngularVelocity;

            var position = throwingEv.Position;

            rigidbody.position = position;
            rigidbody.linearVelocity = velocity;
            rigidbody.angularVelocity = angular;

            if (rigidbody.angularVelocity.magnitude > rigidbody.maxAngularVelocity)
                rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;

            var threwArgs = new PlayerThrewItemEventArgs(player.ReferenceHub, pickup, rigidbody);

            PlayerEvents.OnThrewItem(threwArgs);

            if (customItem != null)
            {
                var dataTemp = customItemTracker.Data;

                customItem.OnThrewItem(threwArgs, ref dataTemp);
                customItemTracker.Data = dataTemp;
            }
        }

        return false;
    }
}