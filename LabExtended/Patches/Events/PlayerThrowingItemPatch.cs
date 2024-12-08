using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using PlayerRoles.FirstPersonControl;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    public static class PlayerThrowingItemPatch
    {
        [HookPatch(typeof(PlayerDropItemEvent))]
        [HookPatch(typeof(PlayerThrowItemEvent))]
        [HookPatch(typeof(PlayerThrowingItemArgs))]
        [HookPatch(typeof(PlayerDroppingItemArgs))]
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.UserCode_CmdDropItem__UInt16__Boolean))]
        public static bool Prefix(Inventory __instance, ushort itemSerial, bool tryThrow)
        {
            var player = ExPlayer.Get(__instance._hub);

            if (player is null)
                return true;

            if (!player.Switches.CanDropItems)
                return false;

            if (!__instance.UserInventory.Items.TryGetValue(itemSerial, out var item) || !item.AllowHolster
                || !EventManager.ExecuteEvent(new PlayerDropItemEvent(__instance._hub, item)))
                return false;

            var droppingEv = new PlayerDroppingItemArgs(player, item, tryThrow);

            if (!HookRunner.RunEvent(droppingEv, true))
                return false;

            ItemPickupBase pickup = __instance.ServerDropItem(itemSerial);

            if (item is Scp330Bag)
                player.Inventory._bag = null;

            __instance.SendItemsNextFrame = true;

            tryThrow = droppingEv.IsThrow;

            if (pickup is null)
                return false;

            player.Inventory._droppedItems.Add(pickup);

            if (player.Switches.CanThrowItems && tryThrow && pickup.TryGetComponent<Rigidbody>(out var rigidbody)
                && EventManager.ExecuteEvent(new PlayerThrowItemEvent(__instance._hub, item, rigidbody)))
            {
                var velocity = __instance._hub.GetVelocity();
                var angular = Vector3.Lerp(item.ThrowSettings.RandomTorqueA, item.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

                velocity = velocity / 3f + __instance._hub.PlayerCameraReference.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, rigidbody.mass)) + 0.3f);

                velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
                velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
                velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

                var throwingEv = new PlayerThrowingItemArgs(player, item, pickup, rigidbody, __instance._hub.PlayerCameraReference.position, velocity, angular);

                if (!HookRunner.RunEvent(throwingEv, true))
                    return false;

                rigidbody.position = throwingEv.Position;
                rigidbody.velocity = throwingEv.Velocity;
                rigidbody.angularVelocity = throwingEv.AngularVelocity;

                if (rigidbody.angularVelocity.magnitude > rigidbody.maxAngularVelocity)
                    rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
            }

            return false;
        }
    }
}