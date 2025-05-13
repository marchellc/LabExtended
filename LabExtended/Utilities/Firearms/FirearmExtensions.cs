using InventorySystem.Items.Autosync;

using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Scp127;
using MEC;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Common firearm extensions.
/// </summary>
public static class FirearmExtensions
{
    /// <summary>
    /// Reloads the firearm's ammo.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="emptyMagazine">Whether or not an empty magazine should be inserted.</param>
    /// <param name="playAnimationToOwner">Whether or not the animation should be shown to the firearm's owner.</param>
    /// <param name="playAnimationToEveryone">Whether or not the animation should be shown to other players.</param>
    /// <returns>true if the firearm's ammo was reloaded.</returns>
    public static bool ReloadAmmo(this Firearm firearm, bool emptyMagazine = false, bool playAnimationToOwner = false, bool playAnimationToEveryone = false)
    {
        if (!firearm.TryGetModule<MagazineModule>(out var magazineModule))
            return false;

        magazineModule.ServerRemoveMagazine();

        if ((playAnimationToOwner || playAnimationToEveryone) && firearm.TryGetModule<AnimatorReloaderModuleBase>(out var reloaderModuleBase))
        {
            reloaderModuleBase.SendRpc(x =>
            {
                x.WriteSubheader(AnimatorReloaderModuleBase.ReloaderMessageHeader.Reload);

                if (reloaderModuleBase._randomize)
                    x.WriteByte((byte)UnityEngine.Random.Range(byte.MinValue, byte.MaxValue + 1));
            }, playAnimationToEveryone);
        }

        Timing.CallDelayed(0.5f, () =>
        {
            if (emptyMagazine)
                magazineModule.ServerInsertEmptyMagazine();
            else
                magazineModule.ServerInsertMagazine();
        });

        return true;
    }
}