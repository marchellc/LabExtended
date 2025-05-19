using HarmonyLib;

using InventorySystem;
using InventorySystem.Items.Autosync;
using InventorySystem.Items.Firearms.Ammo;

using PlayerRoles.Spectating;

using Utils.Networking;

namespace LabExtended.Patches.Fixes;

/// <summary>
/// Patches a NullReferenceException that happens in <see cref="ReserveAmmoSync.UpdateDelta"/> when a destroyed ReferenceHub instance is encountered.
/// </summary>
public static class ReserveAmmoSyncNrePatch
{
    [HarmonyPatch(typeof(ReserveAmmoSync), nameof(ReserveAmmoSync.UpdateDelta))]
    private static bool Prefix()
    {
        try
        {
            foreach (var activeInstance in AutosyncItem.Instances)
            {
                try
                {
                    if (activeInstance == null)
                        continue;

                    if (!ReserveAmmoSync.TryUnpack(activeInstance, out var owner, out var ammoType))
                        continue;

                    if (owner == null || owner?.gameObject == null)
                        continue;

                    var currentAmmo = owner.inventory.GetCurAmmo(ammoType);
                    var syncAmmo = ReserveAmmoSync.ServerLastSent.GetOrAdd(owner, () => new ReserveAmmoSync.LastSent());

                    if (syncAmmo.AmmoCount != currentAmmo || syncAmmo.AmmoType != ammoType)
                    {
                        syncAmmo.AmmoType = ammoType;
                        syncAmmo.AmmoCount = currentAmmo;

                        new ReserveAmmoSync.ReserveAmmoMessage(owner, ammoType).SendToHubsConditionally(x =>
                            x.roleManager.CurrentRole is SpectatorRole);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        return false;
    }
}