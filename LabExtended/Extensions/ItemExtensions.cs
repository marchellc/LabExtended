using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Jailbird;

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

        public static void SetupItem(this ItemBase item, ReferenceHub owner)
        {
            if (item.Owner != null && item.Owner != owner)
            {
                item.OwnerInventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
                item.Owner = null;
            }

            item.Owner = owner;
            item.OnAdded(null);

            if (item is IAcquisitionConfirmationTrigger confirmationTrigger)
            {
                confirmationTrigger.AcquisitionAlreadyReceived = true;
                confirmationTrigger.ServerConfirmAcqusition();
            }

            if (item is Firearm firearm)
            {
                var preferenceCode = uint.MinValue;
                var flags = firearm.Status.Flags;

                if (!AttachmentsServerHandler.PlayerPreferences.TryGetValue(owner, out var preferences) || !preferences.TryGetValue(item.ItemTypeId, out preferenceCode))
                    preferenceCode = AttachmentsUtils.GetRandomAttachmentsCode(item.ItemTypeId);

                if (firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
                    flags |= FirearmStatusFlags.FlashlightEnabled;

                firearm.Status = new FirearmStatus(firearm.AmmoManagerModule.MaxAmmo, flags | FirearmStatusFlags.MagazineInserted, preferenceCode);
            }

            if (item is JailbirdItem jailbird)
                jailbird.ServerReset();
        }
    }
}