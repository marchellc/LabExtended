using InventorySystem;
using InventorySystem.Items;

namespace LabExtended.Extensions
{
    public static class ItemExtensions
    {
        public static T GetInstance<T>(this ItemType itemType) where T : ItemBase
        {
            if (!InventoryItemLoader.TryGetItem<T>(itemType, out var result))
                return null;

            var item = UnityEngine.Object.Instantiate(result);

            item.ItemSerial = ItemSerialGenerator.GenerateNext();
            return item;
        }
    }
}