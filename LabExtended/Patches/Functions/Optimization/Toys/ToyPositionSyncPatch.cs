using AdminToys;

using HarmonyLib;

using LabExtended.Core.Optimization.Toys;

namespace LabExtended.Patches.Functions.Optimization.Toys;

public static class ToyPositionSyncPatch
{
    [HarmonyPatch(typeof(AdminToyBase), nameof(AdminToyBase.LateUpdate))]
    public static bool Prefix(AdminToyBase __instance)
    {
        if (__instance.IsStatic) return false;

        if (ToyPositionSync.ShouldSync(__instance.Position, __instance.transform.position))
            ToyPositionSync.SyncPosition(__instance);
        
        if (ToyPositionSync.ShouldSync(__instance.Rotation, __instance.transform.rotation))
            ToyPositionSync.SyncRotation(__instance);
        
        if (__instance.Scale != __instance.transform.localScale)
            ToyPositionSync.SyncScale(__instance);
        
        return false;
    }
}