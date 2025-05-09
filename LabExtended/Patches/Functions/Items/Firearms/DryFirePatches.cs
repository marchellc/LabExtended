using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API.CustomFirearms;
using LabExtended.API.CustomItems;

using LabExtended.Extensions;

namespace LabExtended.Patches.Functions.Items.Firearms;

public static class DryFirePatches
{
    [HarmonyPatch(typeof(DoubleActionModule), nameof(DoubleActionModule.FireDry))]
    public static bool DoubleActionPrefix(DoubleActionModule __instance)
    {
        var customFirearm = __instance.Firearm.GetTracker().CustomItem as CustomFirearmInstance;

        if (customFirearm is not null && !customFirearm.OnDryFiring())
            return false;

        var dryFiringEventArgs = new PlayerDryFiringWeaponEventArgs(__instance.Firearm.Owner, __instance.Firearm);
        
        PlayerEvents.OnDryFiringWeapon(dryFiringEventArgs);

        if (!dryFiringEventArgs.IsAllowed)
            return false;

        __instance.SendRpc(DoubleActionModule.MessageType.RpcDryFire);
        __instance._audioModule.PlayNormal(__instance._dryFireClip);
        
        customFirearm?.OnDryFired();
        
        PlayerEvents.OnDryFiredWeapon(new(__instance.Firearm.Owner, __instance.Firearm));
        return false;
    }
}