using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items.CustomFirearms;

/// <summary>
/// Implements shooting management for Custom Firearms.
/// </summary>
public static class BuckshotHitregFirePatch
{
    [HarmonyPatch(typeof(BuckshotHitreg), nameof(BuckshotHitreg.Fire))]
    private static bool Prefix(BuckshotHitreg __instance)
    {
        var firearms = ListPool<CustomFirearmInventoryBehaviour>.Shared.Rent();
        
        CustomItemUtils.GetInventoryBehavioursNonAlloc(__instance.Firearm.ItemSerial, firearms);

        if (firearms.Count == 0)
        {
            ListPool<CustomFirearmInventoryBehaviour>.Shared.Return(firearms);
            return true;
        }
        
        firearms.ForEach(firearm => firearm.InternalBuckshotFire(__instance));
        return false;
    }
}