using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API.CustomFirearms;
using LabExtended.API.CustomItems;

using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items.Firearms;

/// <summary>
/// Implements custom ammo loading for custom firearms
/// </summary>
public static class AmmoLoadPatches
{
    [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.ServerUnloadChambered), typeof(int))]
    public static bool AutomaticActionPrefix(AutomaticActionModule __instance, int insertionLimit)
    {
        if (!CustomItemManager.InventoryItems.TryGetValue<CustomFirearmInstance>(__instance.Firearm,
                out var customFirearm) || !customFirearm.CustomData.AmmoType.HasValue
            || customFirearm.CustomData.AmmoType.Value.IsAmmo())
            return true;
        
        if (!__instance.Firearm.TryGetModule<IPrimaryAmmoContainerModule>(out var primaryAmmoContainer))
            return false;

        customFirearm.UnloadedAmmo = customFirearm.LoadedAmmo;
        
        customFirearm.LoadedAmmo = 0;
        customFirearm.OnUnloaded();

        return false;
    }
}