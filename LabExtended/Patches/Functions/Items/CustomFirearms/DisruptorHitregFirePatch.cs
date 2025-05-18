using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomItems;

using NorthwoodLib.Pools;

namespace LabExtended.Patches.Functions.Items.CustomFirearms;

/// <summary>
/// Implements shooting management for Custom Firearms.
/// </summary>
public static class DisruptorHitregFirePatch
{
    [HarmonyPatch(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.Fire))]
    private static bool Prefix(DisruptorHitregModule __instance)
    {
        var firearms = ListPool<CustomFirearmInventoryBehaviour>.Shared.Rent();
        
        CustomItemUtils.GetInventoryBehavioursNonAlloc(__instance.Firearm.ItemSerial, firearms);

        if (firearms.Count == 0)
        {
            ListPool<CustomFirearmInventoryBehaviour>.Shared.Return(firearms);
            return true;
        }
        
        firearms.ForEach(firearm => firearm.InternalDisruptorFire(__instance));
        return false;
    }
}