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
        var firearms = ListPool<CustomFirearmInventoryBehaviour>.Shared.Rent();
        
        CustomItemUtils.GetInventoryBehavioursNonAlloc(__instance.Firearm.ItemSerial, firearms);

        if (firearms.Count == 0)
        {
            ListPool<CustomFirearmInventoryBehaviour>.Shared.Return(firearms);
            return true;
        }
        
        firearms.ForEach(firearm => firearm.InternalSingleBarrelFire(__instance));
        return false;
    }
}