using HarmonyLib;

using InventorySystem;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Prevents the base-game from refreshing modifiers.
    /// </summary>
    public static class DisableRefreshModifiersPatch
    {
        [HarmonyPatch(typeof(Inventory), nameof(Inventory.RefreshModifiers))]
        private static bool Prefix(Inventory __instance)
            => false;
    }
}