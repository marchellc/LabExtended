using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabApi.Features.Console;
using LabApi.Features.Wrappers;

namespace LabExtended.Patches.Fixes
{
    /// <summary>
    /// Supresses certain logs from the LabAPI that are deemed unnecessary or overly verbose.
    /// </summary>
    public static class LabApiSupressLogsPatch
    {
        private static HashSet<Type> missingWrappersLogged = new();

        [HarmonyPatch(typeof(Pickup), nameof(Pickup.CreateItemWrapper))]
        private static bool PickupWrapperPrefix(ItemPickupBase pickupBase, ref Pickup __result)
        {
            var type = pickupBase.GetType();

            if (Pickup.TypeWrappers.TryGetValue(type, out var constructor))
            {
                __result = constructor(pickupBase);
            }
            else if (missingWrappersLogged.Add(type))
            {
                ServerConsole.AddLog($"[LabApi] Failed to find pickup wrapper for type {type}", ConsoleColor.Yellow);
            }

            return false;
        }
    }
}