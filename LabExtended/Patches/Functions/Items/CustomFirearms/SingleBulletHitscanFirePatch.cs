using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items.CustomFirearms;

/// <summary>
/// Implements shooting management for Custom Firearms.
/// </summary>
public static class SingleBulletHitregFirePatch
{
    [HarmonyPatch(typeof(SingleBulletHitscan), nameof(SingleBulletHitscan.Fire))]
    private static bool Prefix(SingleBulletHitscan __instance)
    {
        return !CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(__instance.ItemSerial,
            firearm => firearm.InternalSingleBarrelFire(__instance));
    }
}