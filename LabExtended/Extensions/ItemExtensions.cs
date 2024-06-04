using InventorySystem;
using InventorySystem.Items;

namespace LabExtended.Extensions
{
    /// <summary>
    /// A class that holds extensions for the <see cref="ItemBase"/> and <see cref="Inventory"/> class.
    /// </summary>
    public static class ItemExtensions
    {
        /// <summary>
        /// Gets an instance of an item.
        /// </summary>
        /// <typeparam name="T">The type of the item to get.</typeparam>
        /// <param name="itemType">The type of the item to get.</param>
        /// <returns>The item's instance, if succesfull. Otherwise null.</returns>
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