using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Items.Custom.Pickup;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using Mirror;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

namespace LabExtended.API.Items.Custom.Item
{
    public class CustomItemInventoryBehaviour : CustomItemBehaviour
    {
        public override ExPlayer Owner => ExPlayer.Get(Item?.Owner);
        public override bool IsEnabled => Item?.Owner != null;

        public ItemBase Item { get; internal set; }

        public bool IsSelected => Item != null && Item.Owner != null && Item.OwnerInventory.CurInstance != null && Item.OwnerInventory.CurInstance == Item;

        public virtual void OnRemoving() { }
        public virtual void OnRemoved() { }

        public virtual void OnAdding(PlayerPickingUpItemArgs args) { }
        public virtual void OnAdded(PlayerPickingUpItemArgs args) { }

        public virtual void OnSelected(PlayerSelectingItemArgs args) { }
        public virtual void OnSelecting(PlayerSelectedItemArgs args) { }

        public virtual void OnDeselecting(PlayerSelectingItemArgs args) { }
        public virtual void OnDeselected(PlayerSelectedItemArgs args) { }

        public virtual void OnThrowing(PlayerThrowingItemArgs args) { }
        public virtual void OnThrown(PlayerThrowingItemArgs args) { }

        public virtual void OnDropping(PlayerDroppingItemArgs args) { }

        public void Select()
        {
            if (IsSelected)
                return;

            Item.OwnerInventory.UserCode_CmdSelectItem__UInt16(ItemSerial);
        }

        public void Remove()
        {
            if (Item is null)
                return;

            OnRemoving();

            Item.DestroyItem();
            Item = null;

            CustomItem.Items.Remove(this);
            CustomItemManager._inventoryItems.Remove(ItemSerial);

            OnRemoved();

            InternalOnDisabled();
        }

        public bool Throw(Vector3? position = null, Vector3? velocity = null, Vector3? scale = null, Quaternion? rotation = null)
        {
            if (!IsEnabled)
                throw new Exception("This behaviour instance has been disabled");

            if (!CustomItem.PickupType.HasValue)
                return false;

            var dropRotation = rotation.HasValue ? rotation.Value : Item.Owner.PlayerCameraReference.rotation;
            var dropPosition = position.HasValue ? position.Value : Item.Owner.transform.position;
            var dropVelocity = velocity.HasValue ? velocity.Value : Item.Owner.GetVelocity();
            var dropScale = scale.HasValue ? scale.Value : Vector3.one;

            var dropEvent = new PlayerDroppingItemArgs(Owner, Item, true);

            if (!HookRunner.RunEvent(dropEvent, true))
                return false;

            OnDropping(dropEvent);

            if (!dropEvent.IsAllowed)
                return false;

            var dropPickup = CustomItem.PickupType.Value.GetPickupInstance<ItemPickupBase>(dropPosition, dropScale, dropRotation, ItemSerial, false);
            var dropRigidbody = dropPickup.GetRigidbody();

            dropVelocity = dropVelocity / 3f + Owner.CameraTransform.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, dropRigidbody.mass)) + 0.3f);
            dropVelocity.x = Mathf.Max(Mathf.Abs(dropVelocity.x), Mathf.Abs(dropVelocity.x)) * (!(dropVelocity.x < 0f) ? 1 : -1);
            dropVelocity.y = Mathf.Max(Mathf.Abs(dropVelocity.y), Mathf.Abs(dropVelocity.y)) * (!(dropVelocity.y < 0f) ? 1 : -1);
            dropVelocity.z = Mathf.Max(Mathf.Abs(dropVelocity.z), Mathf.Abs(dropVelocity.z)) * (!(dropVelocity.z < 0f) ? 1 : -1);

            var angularVelocity = Vector3.Lerp(Item.ThrowSettings.RandomTorqueA, Item.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);
            var throwEvent = new PlayerThrowingItemArgs(Owner, Item, dropPickup, dropRigidbody, dropPosition, dropVelocity, angularVelocity);

            if (!HookRunner.RunEvent(throwEvent, true))
            {
                UnityEngine.Object.Destroy(dropPickup.gameObject);
                return false;
            }

            OnThrowing(throwEvent);

            if (!throwEvent.IsAllowed)
            {
                UnityEngine.Object.Destroy(dropPickup.gameObject);
                return false;
            }

            dropRigidbody.position = throwEvent.Position;
            dropRigidbody.velocity = throwEvent.Velocity;
            dropRigidbody.angularVelocity = throwEvent.AngularVelocity;

            if (dropRigidbody.angularVelocity.magnitude > dropRigidbody.maxAngularVelocity)
                dropRigidbody.maxAngularVelocity = dropRigidbody.angularVelocity.magnitude;

            NetworkServer.Spawn(dropPickup.gameObject);

            var dropBehaviour = CustomItem.CreatePickupBehaviour<CustomItemPickupBehaviour>(ItemSerial);

            dropBehaviour.Pickup = dropPickup;
            dropBehaviour.InternalOnEnabled();

            if (Item != null)
            {
                Item.DestroyItem();
                Item = null;
            }

            CustomItem.Items.Remove(this);
            CustomItemManager._inventoryItems.Remove(ItemSerial);

            OnThrown(throwEvent);

            InternalOnDisabled();
            return true;
        }

        public bool Drop(Vector3? position = null, Vector3? scale = null, Quaternion? rotation = null)
        {
            if (!IsEnabled)
                throw new Exception("This behaviour instance has been disabled");

            if (!CustomItem.PickupType.HasValue)
                return false;

            var dropRotation = rotation.HasValue ? rotation.Value : Item.Owner.PlayerCameraReference.rotation;
            var dropPosition = position.HasValue ? position.Value : Item.Owner.transform.position;
            var dropScale = scale.HasValue ? scale.Value : Vector3.one;

            var dropEvent = new PlayerDroppingItemArgs(Owner, Item, false);

            if (!HookRunner.RunEvent(dropEvent, true))
                return false;

            OnDropping(dropEvent);

            if (!dropEvent.IsAllowed)
                return false;

            var dropPickup = CustomItem.PickupType.Value.GetPickupInstance<ItemPickupBase>(dropPosition, dropScale, dropRotation, ItemSerial, true);
            var dropBehaviour = CustomItem.CreatePickupBehaviour<CustomItemPickupBehaviour>(ItemSerial);

            dropBehaviour.Pickup = dropPickup;

            if (Item != null)
            {
                Item.DestroyItem();
                Item = null;
            }

            CustomItem.Items.Remove(this);
            CustomItemManager._inventoryItems.Remove(ItemSerial);

            InternalOnDisabled();

            dropBehaviour.InternalOnEnabled();
            return true;
        }

        internal virtual void InternalOnPickedUp(CustomItemPickupBehaviour customItemPickupBehaviour) { }
    }
}
