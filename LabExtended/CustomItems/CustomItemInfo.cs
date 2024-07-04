using LabExtended.CustomItems.Enums;

namespace LabExtended.CustomItems
{
    public struct CustomItemInfo
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public ItemType InventoryType { get; }
        public CustomItemFlags ItemFlags { get; }

        public CustomItemPickupInfo PickupInfo { get; }

        public Type Type { get; }

        public CustomItemInfo(Type type, string id, string name, string description, ItemType inventoryType, CustomItemFlags itemFlags, CustomItemPickupInfo pickupInfo)
        {
            Type = type;

            Id = id;
            Name = name;
            Description = description;
            ItemFlags = itemFlags;
            InventoryType = inventoryType;
            PickupInfo = pickupInfo;
        }
    }
}