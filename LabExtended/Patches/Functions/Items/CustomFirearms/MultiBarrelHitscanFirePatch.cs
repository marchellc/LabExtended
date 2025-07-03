using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items.CustomFirearms;

/// <summary>
/// Implements shooting management for Custom Firearms.
/// </summary>
public static class MultiBarrelHitregFirePatch
{
    [HarmonyPatch(typeof(MultiBarrelHitscan), nameof(MultiBarrelHitscan.Fire))]
    private static bool Prefix(MultiBarrelHitscan __instance)
    {
        return !CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(__instance.Firearm.ItemSerial,
            firearm => firearm.InternalMultiBarrelFire(__instance));
    }
}