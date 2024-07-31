using LabExtended.API.CustomItems.Enums;
using LabExtended.API.CustomItems.Info;

namespace LabExtended.API.CustomItems.Firearms
{
    public class CustomFirearmInfo : CustomItemInfo
    {
        public ItemType AmmoType { get; set; } = ItemType.None;

        public CustomFirearmFlags FirearmFlags { get; set; } = CustomFirearmFlags.None;

        public byte MaxAmmo { get; set; } = 0;
        public byte StartAmmo { get; set; } = 0;

        public CustomFirearmInfo(Type type, string id, string name, string description, ItemType inventoryType, CustomItemFlags itemFlags, CustomItemPickupInfo pickupInfo)
            : base(type, id, name, description, inventoryType, itemFlags, pickupInfo) { }
    }
}