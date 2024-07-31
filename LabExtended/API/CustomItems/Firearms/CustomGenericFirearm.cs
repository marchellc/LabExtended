using InventorySystem.Items.Firearms;

namespace LabExtended.API.CustomItems.Firearms
{
    public class CustomGenericFirearm<TItem, TPickup> : CustomFirearm
        where TItem : Firearm
        where TPickup : FirearmPickup
    {
        public TItem CastItem
        {
            get
            {
                if (Item is null || Item is not TItem castItem)
                    return null;

                return castItem;
            }
        }

        public TPickup CastPickup
        {
            get
            {
                if (Pickup is null || Pickup is not TPickup castPickup)
                    return null;

                return castPickup;
            }
        }
    }
}