using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

namespace LabExtended.Patches.Functions.Items.CustomFirearms;

/// <summary>
/// Implements shooting management for Custom Firearms.
/// </summary>
public static class BuckshotHitregFirePatch
{
    [HarmonyPatch(typeof(BuckshotHitreg), nameof(BuckshotHitreg.Fire))]
    private static bool Prefix(BuckshotHitreg __instance)
    {
        return !CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(__instance.Firearm.ItemSerial,
                firearm => firearm.InternalBuckshotFire(__instance));
    }
}