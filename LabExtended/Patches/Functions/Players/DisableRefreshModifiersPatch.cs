using HarmonyLib;

namespace LabExtended.Patches.Functions.Players
{
    /// <summary>
    /// Prevents the <see cref="InventorySystem.Inventory.RefreshModifiers"/> method from executing.
    /// </summary>
    public static class DisableRefreshModifiersPatch
    {
        [HarmonyPatch(typeof(InventorySystem.Inventory), nameof(InventorySystem.Inventory.RefreshModifiers))]
        private static bool Prefix() => false;
    }
}