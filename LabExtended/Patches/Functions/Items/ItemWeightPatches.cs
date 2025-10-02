using HarmonyLib;

using InventorySystem.Items.Armor;
using InventorySystem.Items.Coin;
using InventorySystem.Items.DebugTools;
using InventorySystem.Items.Usables;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Ammo;

using InventorySystem.Items.FlamingoTapePlayer;
using InventorySystem.Items.Jailbird;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.MicroHID;
using InventorySystem.Items.Radio;
using InventorySystem.Items.ThrowableProjectiles;

using InventorySystem.Items.ToggleableLights.Flashlight;
using InventorySystem.Items.ToggleableLights.Lantern;

using LabExtended.API.Custom.Items;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements custom item weight for inventory items.
/// </summary>
public static class ItemWeightPatches
{
    /// <summary>
    /// Gets the highest item ID.
    /// </summary>
    public const int HighestItemId = 66;

    private static float[] customWeightArray;

    /// <summary>
    /// Gets the array of custom weight values associated with each item identifier. Item IDs are used as indexes.
    /// </summary>
    /// <remarks>The returned array is initialized with a length equal to the highest item identifier, and
    /// each element is set to -1 if not previously assigned. The array is shared across all usages and may be modified
    /// by consumers. Thread safety is not guaranteed.</remarks>
    public static float[] GlobalCustomWeight
    {
        get
        {
            if (customWeightArray is null)
            {
                customWeightArray = new float[HighestItemId];

                for (var x = 0; x < HighestItemId; x++)
                    customWeightArray[x] = -1f;
            }

            return customWeightArray;
        }
    }

    /// <summary>
    /// Gets a per-serial list of custom item weights.
    /// </summary>
    public static Dictionary<ushort, float> SerialCustomWeight { get; } = new();

    /// <summary>
    /// Sets the custom weight value for the specified item type.
    /// </summary>
    /// <param name="type">The item type for which to set the custom weight. Must be a valid, non-negative value.</param>
    /// <param name="weight">The weight value to assign to the specified item type.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="type"/> is less than zero.</exception>
    public static void SetWeight(ItemType type, float weight)
    {
        if (type < 0)
            throw new ArgumentOutOfRangeException(nameof(type));

        GlobalCustomWeight[(int)type] = weight;
    }

    /// <summary>
    /// Sets the custom weight value for the specified serial identifier.
    /// </summary>
    /// <param name="serial">The unique serial identifier for which to set the custom weight.</param>
    /// <param name="weight">The weight value to associate with the specified serial identifier.</param>
    public static void SetWeight(ushort serial, float weight)
    {
        SerialCustomWeight[serial] = weight;
    }

    /// <summary>
    /// Calculates the effective weight for an item, considering custom weights defined by item type, serial number, or
    /// custom item data.
    /// </summary>
    /// <remarks>Custom weights are applied in the following order of precedence: custom item data,
    /// serial-specific weight, then type-specific weight. If none are defined, the base weight is used.</remarks>
    /// <param name="serial">The unique serial number of the item. If nonzero and a custom weight is defined for this serial, it overrides
    /// other weight values.</param>
    /// <param name="type">The type of the item. If a custom weight is defined for this type, it overrides the base weight unless a
    /// serial-specific or custom item weight is present.</param>
    /// <param name="baseWeight">The default weight of the item to use if no custom weight is defined for the type, serial, or custom item data.</param>
    /// <returns>The calculated weight for the item, reflecting any applicable customizations. Returns the base weight if no
    /// customizations are found.</returns>
    public static float GetWeight(ItemType type, ushort serial, float baseWeight)
    {
        var globalWeight = GlobalCustomWeight[(int)type];

        if (type > 0 && globalWeight != -1f)
            baseWeight = globalWeight;

        if (serial != 0 && SerialCustomWeight.TryGetValue(serial, out var serialWeight))
            baseWeight = serialWeight;

        if (CustomItem.IsCustomItem(serial, out var customItem)
            && customItem.Weight != -1f)
            baseWeight = customItem.Weight;

        return baseWeight;
    }

    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.Weight), MethodType.Getter)]
    private static bool BodyArmorPrefix(BodyArmor __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
    
    [HarmonyPatch(typeof(Coin), nameof(Coin.Weight), MethodType.Getter)]
    private static bool CoinPrefix(Coin __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.0025f);
        return false;
    }
    
    [HarmonyPatch(typeof(RagdollMover), nameof(RagdollMover.Weight), MethodType.Getter)]
    private static bool RagdollMoverPrefix(RagdollMover __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 1f);
        return false;
    }
    
    [HarmonyPatch(typeof(AmmoItem), nameof(AmmoItem.Weight), MethodType.Getter)]
    private static bool AmmoPrefix(AmmoItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;

        var weight = 0.25f;

        if (__instance.PickupDropModel is AmmoPickup ammoPickup)
            weight += ammoPickup.SavedAmmo * 0.01f;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, weight);
        return false;
    }
    
    [HarmonyPatch(typeof(Firearm), nameof(Firearm.Weight), MethodType.Getter)]
    private static bool FirearmPrefix(Firearm __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance.TotalWeightKg());
        return false;
    }
    
    [HarmonyPatch(typeof(TapeItem), nameof(TapeItem.Weight), MethodType.Getter)]
    private static bool TapePlayerPrefix(TapeItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.35f);
        return false;
    }
    
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.Weight), MethodType.Getter)]
    private static bool JailbirdPrefix(JailbirdItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 1.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(KeycardItem), nameof(KeycardItem.Weight), MethodType.Getter)]
    private static bool KeycardPrefix(KeycardItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 
            0.01f + __instance.KeycardGfx.ExtraWeight);
        return false;
    }
    
    [HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.Weight), MethodType.Getter)]
    private static bool MicroHidPrefix(MicroHIDItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 25.1f);
        return false;
    }
    
    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.Weight), MethodType.Getter)]
    private static bool RadioPrefix(RadioItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 1.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.Weight), MethodType.Getter)]
    private static bool ThrowablePrefix(ThrowableItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
    
    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.Weight), MethodType.Getter)]
    private static bool FlashlightPrefix(FlashlightItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(LanternItem), nameof(LanternItem.Weight), MethodType.Getter)]
    private static bool LanternPrefix(LanternItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(UsableItem), nameof(UsableItem.Weight), MethodType.Getter)]
    private static bool UsablePrefix(UsableItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = GetWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
}