using InventorySystem.Items;
using InventorySystem.Items.Pickups;

namespace LabExtended.API.CustomItems
{
    public class CustomItemGeneric<TItem, TPickup> : CustomItem
        where TItem : ItemBase
        where TPickup : ItemPickupBase
    {
        public TItem CastItem
        {
            get
            {
                if (Item is null)
                    return null;

                if (Item is not TItem castItem)
                    throw new Exception($"Item {Item.GetType().FullName} cannot be cast to {typeof(TItem).FullName}");

                return castItem;
            }
        }

        public TPickup CastPickup
        {
            get
            {
                if (Pickup is null)
                    return null;

                if (Pickup is not TPickup castPickup)
                    throw new Exception($"Pickup {Pickup.GetType().FullName} cannot be cast to {typeof(TPickup).FullName}");

                return castPickup;
            }
        }
    }
}