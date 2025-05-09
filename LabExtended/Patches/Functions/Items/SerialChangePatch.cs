using HarmonyLib;

using InventorySystem.Items;

using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Allows ItemTracker to track serial changes.
/// </summary>
public static class SerialChangePatch
{
    [HarmonyPatch(typeof(ItemBase), nameof(ItemBase.ItemSerial), MethodType.Setter)]
    private static bool Prefix(ItemBase __instance, ref ushort value)
    {
        var tracker = __instance.GetTracker();
        
        tracker.SetSerial(value);
        return true;
    }
}