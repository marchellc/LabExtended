using HarmonyLib;

using InventorySystem.Items.Armor;
using InventorySystem.Items.Coin;
using InventorySystem.Items.DebugTools;
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
using InventorySystem.Items.Usables;

using LabExtended.API.CustomItems;

namespace LabExtended.Patches.Functions.Items.CustomItems;

/// <summary>
/// Implements custom item weight for inventory items.
/// </summary>
public static class CustomItemInventoryWeightPatches
{
    [HarmonyPatch(typeof(BodyArmor), nameof(BodyArmor.Weight), MethodType.Getter)]
    private static bool BodyArmorPrefix(BodyArmor __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
    
    [HarmonyPatch(typeof(Coin), nameof(Coin.Weight), MethodType.Getter)]
    private static bool CoinPrefix(Coin __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.0025f);
        return false;
    }
    
    [HarmonyPatch(typeof(RagdollMover), nameof(RagdollMover.Weight), MethodType.Getter)]
    private static bool RagdollMoverPrefix(RagdollMover __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 1f);
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
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, weight);
        return false;
    }
    
    [HarmonyPatch(typeof(Firearm), nameof(Firearm.Weight), MethodType.Getter)]
    private static bool FirearmPrefix(Firearm __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance.TotalWeightKg());
        return false;
    }
    
    [HarmonyPatch(typeof(TapeItem), nameof(TapeItem.Weight), MethodType.Getter)]
    private static bool TapePlayerPrefix(TapeItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.35f);
        return false;
    }
    
    [HarmonyPatch(typeof(JailbirdItem), nameof(JailbirdItem.Weight), MethodType.Getter)]
    private static bool JailbirdPrefix(JailbirdItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 1.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(KeycardItem), nameof(KeycardItem.Weight), MethodType.Getter)]
    private static bool KeycardPrefix(KeycardItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 
            0.01f + __instance.KeycardGfx.ExtraWeight);
        return false;
    }
    
    [HarmonyPatch(typeof(MicroHIDItem), nameof(MicroHIDItem.Weight), MethodType.Getter)]
    private static bool MicroHidPrefix(MicroHIDItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 25.1f);
        return false;
    }
    
    [HarmonyPatch(typeof(RadioItem), nameof(RadioItem.Weight), MethodType.Getter)]
    private static bool RadioPrefix(RadioItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 1.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(ThrowableItem), nameof(ThrowableItem.Weight), MethodType.Getter)]
    private static bool ThrowablePrefix(ThrowableItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
    
    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.Weight), MethodType.Getter)]
    private static bool FlashlightPrefix(FlashlightItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(LanternItem), nameof(LanternItem.Weight), MethodType.Getter)]
    private static bool LanternPrefix(LanternItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, 0.7f);
        return false;
    }
    
    [HarmonyPatch(typeof(UsableItem), nameof(UsableItem.Weight), MethodType.Getter)]
    private static bool UsablePrefix(UsableItem __instance, ref float __result)
    {
        if (__instance.ItemSerial == 0)
            return true;
        
        __result = CustomItemUtils.GetInventoryCustomWeight(__instance.ItemTypeId, __instance.ItemSerial, __instance._weight);
        return false;
    }
}