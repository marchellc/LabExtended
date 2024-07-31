using Achievements.Handlers;
using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.Extensions;
using Respawning;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Exists because Northwood for some reason decided to access the <see cref="InventoryItemLoader.AvailableItems"/> dictionary directly instead of using their <see cref="InventoryItemLoader.TryGetItem{T}(ItemType, out T)"/> method ..
    /// </summary>
    public static class DirectItemDictAccessPatch
    {
        public static ItemBase CreateInstance(ItemType type, ushort serial, ReferenceHub owner, ItemBase prefab = null)
        {
            if (prefab is null || !type.TryGetItemPrefab(out prefab))
                return null;

            var instance = UnityEngine.Object.Instantiate(prefab);

            instance.Owner = owner ?? ReferenceHub.HostHub;
            instance.ItemSerial = serial;

            return instance;
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.CreateItemInstance))]
        [HarmonyPrefix]
        public static bool CreateItemInstancePrefix(Inventory __instance, ItemIdentifier identifier, ref ItemBase __result)
        {
            if (!identifier.TypeId.TryGetItemPrefab(out var prefab))
            {
                __result = null;
                return false;
            }

            var instance = UnityEngine.Object.Instantiate(prefab, __instance.ItemWorkspace);

            instance.Owner = __instance._hub;
            instance.ItemSerial = identifier.SerialNumber;

            __result = instance;
            return false;
        }

        [HarmonyPatch(typeof(InventoryExtensions), nameof(InventoryExtensions.ServerAddItem))]
        [HarmonyPrefix]
        public static bool ServerAddItemPrefix(Inventory inv, ItemType type, ushort itemSerial, ItemPickupBase pickup, ref ItemBase __result)
        {
            if (!type.TryGetItemPrefab(out var prefab))
                return false;

            if (inv.UserInventory.Items.Count > 7 && prefab.Category != ItemCategory.Ammo)
                return false;

            if (itemSerial == 0)
                itemSerial = ItemSerialGenerator.GenerateNext();

            __result = CreateInstance(type, itemSerial, inv._hub, prefab);

            if (__result is null)
                return false;

            inv.UserInventory.Items[itemSerial] = __result;
            inv.SendItemsNextFrame = true;

            __result.OnAdded(pickup);

            // OnItemAdded.Invoke
            ItemPickupTokens.OnItemAdded(inv._hub, __result, pickup);
            ItemPickupHandler.OnItemAdded(inv._hub, __result, pickup);

            if (inv.isLocalPlayer && __result is IAcquisitionConfirmationTrigger acquisitionConfirmationTrigger)
            {
                acquisitionConfirmationTrigger.AcquisitionAlreadyReceived = true;
                acquisitionConfirmationTrigger.ServerConfirmAcqusition();
            }

            return false;
        }
    }
}
