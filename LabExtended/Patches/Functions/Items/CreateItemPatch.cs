using HarmonyLib;

using InventorySystem;
using InventorySystem.Items;

using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Provides the <see cref="Created"/> event.
/// </summary>
public static class CreateItemPatch
{
    /// <summary>
    /// Gets called once an item is created.
    /// </summary>
    public static event Action<ItemBase>? Created;
    
    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CreateItemInstance))]
    private static bool Prefix(Inventory __instance, ItemIdentifier identifier, ref ItemBase __result)
    {
        if (!InventoryItemLoader.TryGetItem<ItemBase>(identifier.TypeId, out var prefab))
        {
            __result = null;
            return false;
        }

        __result = UnityEngine.Object.Instantiate(prefab);
        __result.Owner = __instance._hub;
        __result.ItemSerial = identifier.SerialNumber;

        Created?.InvokeSafe(__result);
        return false;
    }
}