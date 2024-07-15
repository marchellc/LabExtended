using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Firearms;

namespace LabExtended.Extensions
{
    public static class CustomItemExtensions
    {
        public static bool TryGetCustomItem(this ItemBase itemBase, out CustomItem customItem)
            => CustomItem.TryGetItem(itemBase, out customItem);

        public static bool TryGetCustomItem<T>(this ItemBase itemBase, out T customItem) where T : CustomItem
            => CustomItem.TryGetItem<T>(itemBase, out customItem);

        public static bool TryGetCustomFirearm(this ItemBase itemBase, out CustomFirearm customFirearm)
            => CustomItem.TryGetItem(itemBase, out customFirearm);

        public static bool TryGetCustomItem(this ItemPickupBase itemPickupBase, out CustomItem customItem)
            => CustomItem.TryGetItem(itemPickupBase, out customItem);

        public static bool TryGetCustomItem<T>(this ItemPickupBase itemPickupBase, out T customItem) where T : CustomItem
            => CustomItem.TryGetItem<T>(itemPickupBase, out customItem);

        public static bool TryGetCustomFirearm(this ItemPickupBase itemPickupBase, out CustomFirearm customFirearm)
            => CustomItem.TryGetItem(itemPickupBase, out customFirearm);

        public static bool IsCustomItem(this ItemBase itemBase)
            => CustomItem.TryGetItem(itemBase, out _);

        public static bool IsCustomItem(this ItemPickupBase itemPickupBase)
            => CustomItem.TryGetItem(itemPickupBase, out _);

        public static bool IsCustomItemSerial(this ushort serialNum)
            => CustomItem.TryGetItem(serialNum, out _);
    }
}