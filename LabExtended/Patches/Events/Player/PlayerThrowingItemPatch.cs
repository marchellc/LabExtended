using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;
using LabExtended.API;
using LabExtended.API.CustomItems;

using LabExtended.Attributes;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Player;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

using PlayerDroppingItemEventArgs = LabExtended.Events.Player.PlayerDroppingItemEventArgs;
using PlayerThrowingItemEventArgs = LabExtended.Events.Player.PlayerThrowingItemEventArgs;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerThrowingItemPatch
    {
        [EventPatch(typeof(PlayerThrowingItemEventArgs), true)]
        [EventPatch(typeof(PlayerDroppingItemEventArgs), true)]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem__UInt16__Boolean))]
        public static bool Prefix(Inventory __instance, ushort itemSerial, bool tryThrow)
        {
            var player = ExPlayer.Get(__instance._hub);

            if (player is null)
                return true;

            if (!player.Toggles.CanDropItems)
                return false;

            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var item) || !item.AllowHolster)
                return false;

            var droppingArgs = new LabApi.Events.Arguments.PlayerEvents.PlayerDroppingItemEventArgs(player.ReferenceHub, item);

            PlayerEvents.OnDroppingItem(droppingArgs);

            if (!droppingArgs.IsAllowed)
                return false;

            var droppingEv = new PlayerDroppingItemEventArgs(player, item, tryThrow);

            if (!ExPlayerEvents.OnDroppingItem(droppingEv))
                return false;

            CustomItemManager.InventoryItems.TryGetValue(item, out var customItemInstance);

            ItemPickupBase pickup = null;

            if (customItemInstance != null)
            {
                if (!customItemInstance.OnDropping(ref tryThrow))
                    return false;

                if (customItemInstance.CustomData.PickupType is ItemType.None)
                    return false;
                
                __instance.ServerRemoveItem(itemSerial, item.PickupDropModel);

                pickup = ExMap.SpawnItem(customItemInstance.CustomData.PickupType, player.Position,
                    customItemInstance.CustomData.PickupScale ?? Vector3.one, player.Rotation, customItemInstance.ItemSerial, 
                    true);
            }
            else
            { 
                pickup = __instance.ServerDropItem(itemSerial);
            }

            __instance.SendItemsNextFrame = true;

            tryThrow = droppingEv.IsThrow;

            if (pickup is null)
                return false;

            player.Inventory._droppedItems.Add(pickup);

            PlayerEvents.OnDroppedItem(new PlayerDroppedItemEventArgs(player.ReferenceHub, pickup));

            if (customItemInstance != null)
            {
                customItemInstance.Item = null;
                customItemInstance.IsHeld = false;
                
                customItemInstance.Pickup = pickup;
                customItemInstance.OnDropped(tryThrow);
                
                CustomItemManager.PickupItems.Add(pickup, customItemInstance);
                CustomItemManager.InventoryItems.Remove(item);

                player.customItems.Remove(item);
                player.Inventory.heldCustomItem = null;
            }

            if (player.Toggles.CanThrowItems && tryThrow && pickup.TryGetRigidbody(out var rigidbody))
            {
                var throwingArgs = new LabApi.Events.Arguments.PlayerEvents.PlayerThrowingItemEventArgs(player.ReferenceHub, pickup, rigidbody);

                PlayerEvents.OnThrowingItem(throwingArgs);

                if (!throwingArgs.IsAllowed)
                    return false;
                
                var velocity = __instance._hub.GetVelocity();
                var angular = Vector3.Lerp(item.ThrowSettings.RandomTorqueA, item.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

                velocity = velocity / 3f + __instance._hub.PlayerCameraReference.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, rigidbody.mass)) + 0.3f);

                velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
                velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
                velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

                var throwingEv = new PlayerThrowingItemEventArgs(player, droppingArgs.Item, Pickup.Get(pickup), rigidbody, 
                    __instance._hub.PlayerCameraReference.position, velocity, angular);

                if (!ExPlayerEvents.OnThrowingItem(throwingEv))
                    return false;

                velocity = throwingEv.Velocity;
                angular = throwingEv.AngularVelocity;

                var position = throwingEv.Position;

                if (customItemInstance != null &&
                    !customItemInstance.OnThrowing(rigidbody, ref position, ref velocity, ref angular))
                    return false;

                rigidbody.position = position;
                rigidbody.velocity = velocity;
                rigidbody.angularVelocity = angular;

                if (rigidbody.angularVelocity.magnitude > rigidbody.maxAngularVelocity)
                    rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
                
                customItemInstance?.OnThrown();

                PlayerEvents.OnThrewItem(new PlayerThrewItemEventArgs(player.ReferenceHub, pickup, rigidbody));
            }

            return false;
        }
    }
}