using LabExtended.API.Collections.Locked;

using LabExtended.API.Items.Custom.Item;
using LabExtended.API.Items.Custom.Pickup;

namespace LabExtended.API.Items.Custom
{
    public abstract class CustomItem
    {
        internal Func<object[], object> pickupConstructor;
        internal Func<object[], object> inventoryConstructor;

        public LockedList<CustomItemInventoryBehaviour> Items { get; } = new LockedList<CustomItemInventoryBehaviour>();
        public LockedList<CustomItemPickupBehaviour> Pickups { get; } = new LockedList<CustomItemPickupBehaviour>();

        public abstract string Name { get; }
        public abstract string Id { get; }

        public virtual ItemType? InventoryType { get; }
        public virtual ItemType? PickupType { get; }

        public virtual Type PickupBehaviourType { get; }
        public virtual Type InventoryBehaviourType { get; }

        public virtual void OnUpdate() { }

        internal T CreateInventoryBehaviour<T>(ushort itemSerial) where T : CustomItemInventoryBehaviour
        {
            if (inventoryConstructor is null)
                throw new Exception($"The inventory behaviour constructor for this CustomItem is null");

            var inventoryBehaviour = inventoryConstructor(Array.Empty<object>());

            if (inventoryBehaviour != null)
            {
                if (inventoryBehaviour is T tBehaviour)
                {
                    SetupBehavior(tBehaviour, itemSerial);
                    return tBehaviour;
                }

                throw new Exception($"Cannot cast inventory behaviour {inventoryBehaviour.GetType().FullName} to {typeof(T).FullName}");
            }

            throw new Exception($"Failed to construct inventory behavior instance.");
        }

        internal T CreatePickupBehaviour<T>(ushort itemSerial) where T : CustomItemPickupBehaviour
        {
            if (inventoryConstructor is null)
                throw new Exception($"The pickup behaviour constructor for this CustomItem is null");

            var pickupBehaviour = pickupConstructor(Array.Empty<object>());

            if (pickupBehaviour != null)
            {
                if (pickupBehaviour is T tBehaviour)
                {
                    SetupBehavior(tBehaviour, itemSerial);
                    return tBehaviour;
                }

                throw new Exception($"Cannot cast pickup behaviour {pickupBehaviour.GetType().FullName} to {typeof(T).FullName}");
            }

            throw new Exception($"Failed to construct pickup behavior instance.");
        }

        private void SetupBehavior(CustomItemBehaviour behaviour, ushort itemSerial)
        {
            if (behaviour is null)
                throw new ArgumentNullException(nameof(behaviour));

            behaviour.CustomItem = this;
            behaviour.ItemSerial = itemSerial;

            behaviour.InternalOnEnabled();

            if (behaviour is CustomItemInventoryBehaviour customItemInventoryBehaviour)
            {
                Items.Add(customItemInventoryBehaviour);
                CustomItemManager._inventoryItems[itemSerial] = customItemInventoryBehaviour;
            }
            else if (behaviour is CustomItemPickupBehaviour customItemPickupBehaviour)
            {
                Pickups.Add(customItemPickupBehaviour);
                CustomItemManager._pickupItems[itemSerial] = customItemPickupBehaviour;
            }
        }
    }
}