using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Items.Custom.Item;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Items.Custom.Pickup
{
    public class CustomItemPickupBehaviour : CustomItemBehaviour
    {
        private Rigidbody _rigidbody;
        private PickupStandardPhysics _standardPhysics;

        public override ExPlayer Owner => Pickup.PreviousOwner.IsSet && Pickup.PreviousOwner.Hub != null && Pickup.PreviousOwner.Hub ? ExPlayer.Get(Pickup.PreviousOwner.Hub) : null;
        public override bool IsEnabled => Pickup != null && Pickup && Pickup.PreviousOwner.IsSet && Pickup.PreviousOwner.Hub != null && Pickup.PreviousOwner.Hub;

        public ItemPickupBase Pickup { get; internal set; }

        public Rigidbody Rigidbody
        {
            get
            {
                if (Pickup is null)
                    return null;

                if (_rigidbody is null)
                    _rigidbody = Pickup.GetRigidbody();

                return _rigidbody;
            }
        }

        public PickupStandardPhysics PhysicsModule
        {
            get
            {
                if (Pickup is null)
                    return null;

                if (_standardPhysics is null)
                    _standardPhysics = Pickup.PhysicsModule as PickupStandardPhysics;

                return _standardPhysics;
            }
        }

        public Vector3 Position
        {
            get => Pickup?.Position ?? Vector3.zero;
            set => Pickup!.Position = value;
        }

        public Vector3 Scale
        {
            get => Pickup?.transform?.localScale ?? Vector3.zero;
            set
            {
                if (Pickup is null)
                    return;

                NetworkServer.UnSpawn(Pickup.gameObject);

                Pickup.transform.localScale = value;

                NetworkServer.Spawn(Pickup.gameObject);
            }
        }

        public Quaternion Rotation
        {
            get => Pickup?.Rotation ?? Quaternion.identity;
            set => Pickup!.Rotation = value;
        }

        public bool IsLocked
        {
            get
            {
                if (Pickup is null)
                    return false;

                return Pickup.Info.Locked;
            }
            set
            {
                if (Pickup is null)
                    return;

                Pickup.Info.Locked = false;
            }
        }

        public bool IsInUse
        {
            get
            {
                if (Pickup is null)
                    return false;

                return Pickup.Info.Locked;
            }
            set
            {
                if (Pickup is null)
                    return;

                Pickup.Info.InUse = value;
            }
        }

        public virtual void OnPickingUp(PlayerPickingUpItemArgs args) { }
        public virtual void OnPickedUp(PlayerPickingUpItemArgs args) { }

        public void Freeeze()
            => Pickup?.FreezePickup();

        public void Unfreeze()
            => Pickup?.UnfreezePickup();

        public bool Give(ExPlayer player, bool teleportToPlayerIfFullInventory = true, bool selectOnPickup = false)
        {
            if (!player)
                throw new ArgumentNullException(nameof(player));

            if (!IsEnabled)
                throw new Exception("This pickup behaviour has been disabled");

            if (!CustomItem.InventoryType.HasValue)
                throw new Exception($"This Custom Item does not have inventory item type set.");

            var pickEvent = new PlayerPickingUpItemArgs(player, Pickup, null, null, null, true);

            if (!HookRunner.RunEvent(pickEvent, true))
                return false;

            OnPickingUp(pickEvent);

            if (!pickEvent.IsAllowed)
                return false;

            if (player.Inventory.ItemCount >= 8)
            {
                if (teleportToPlayerIfFullInventory)
                    Position = player.Position;

                return false;
            }

            var pickItem = CustomItem.InventoryType.Value.GetItemInstance<ItemBase>(ItemSerial);
            var pickBehaviour = CustomItem.CreateInventoryBehaviour<CustomItemInventoryBehaviour>(ItemSerial);

            pickItem.Owner = player.Hub;

            pickBehaviour.Item = pickItem;
            pickBehaviour.InternalOnPickedUp(this);
            pickBehaviour.InternalOnEnabled();

            OnPickedUp(pickEvent);

            CustomItem.Pickups.Remove(this);
            CustomItemManager._pickupItems.Remove(ItemSerial);

            Pickup.DestroySelf();
            Pickup = null;

            _rigidbody = null;
            _standardPhysics = null;

            if (selectOnPickup && !pickBehaviour.IsSelected)
                pickBehaviour.Select();

            InternalOnDisabled();
            return true;
        }

        public void Despawn()
        {
            if (!IsEnabled)
                throw new Exception("This pickup behaviour has been disabled");

            CustomItem.Pickups.Remove(this);
            CustomItemManager._pickupItems.Remove(ItemSerial);

            Pickup?.DestroySelf();
            Pickup = null;

            _rigidbody = null;
            _standardPhysics = null;

            InternalOnDisabled();
        }
    }
}