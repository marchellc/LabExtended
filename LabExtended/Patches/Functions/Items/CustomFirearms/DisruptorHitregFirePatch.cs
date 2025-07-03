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
        return !CustomItemUtils.ProcessEvent<CustomFirearmInventoryBehaviour>(__instance.Firearm.ItemSerial,
            firearm => firearm.InternalDisruptorFire(__instance));
    }
}