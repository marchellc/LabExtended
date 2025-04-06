using CameraShaking;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions to retrieve firearm properties from modules & submodules.
/// </summary>
public static class FirearmPropertyExtensions
{
    #region Cocking
    /// <summary>
    /// Whether or not a specific firearm is cocked.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is cocked</returns>
    public static bool IsCocked(this Firearm firearm)
        => firearm.GetProperty<bool, IActionModule>(module => module.IsCocked());

    /// <summary>
    /// Whether or not the action module of a specific firearm is cocked.
    /// </summary>
    /// <param name="actionModule">The target action module.</param>
    /// <returns>true if the firearm is cocked.</returns>
    public static bool IsCocked(this IActionModule actionModule)
    {
        if (actionModule is null)
            return false;

        if (actionModule is AutomaticActionModule automaticActionModule)
            return automaticActionModule.Cocked;

        if (actionModule is DoubleActionModule doubleActionModule)
            return doubleActionModule.Cocked;

        return false;
    }

    /// <summary>
    /// Sets the cocked status of a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="isCocked">Whether or not the firearm should be cocked.</param>
    /// <returns>true if the firearm's status has changed</returns>
    public static bool SetCocked(this Firearm firearm, bool isCocked)
        => firearm.DoModule<IActionModule>(module => module.SetCocked(isCocked));
    
    /// <summary>
    /// Sets the cocked status of a firearm's action module.
    /// </summary>
    /// <param name="actionModule">The target action module.</param>
    /// <param name="isCocked">Whether or not the firearm should be cocked.</param>
    /// <returns>true if the firearm's status has changed</returns>
    public static bool SetCocked(this IActionModule actionModule, bool isCocked)
    {
        if (actionModule is null)
            return false;

        if (actionModule is AutomaticActionModule automaticActionModule)
        {
            if (automaticActionModule._serverCocked != isCocked)
            {
                automaticActionModule._serverCocked = isCocked;
                automaticActionModule.ServerResync();

                return true;
            }

            return false;
        }

        if (actionModule is DoubleActionModule doubleActionModule
            && doubleActionModule.Cocked != isCocked)
        {
            doubleActionModule.Cocked = isCocked;
            return true;
        }

        return false;
    }
    #endregion

    /// <summary>
    /// Whether or not the firearm is busy.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is busy</returns>
    public static bool IsBusy(this Firearm firearm)
        => firearm.AnyModuleBusy();
    
    /// <summary>
    /// Whether or not the firearm is being reloaded.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is being reloaded</returns>
    public static bool IsReloading(this Firearm firearm)
        => firearm.GetProperty<bool, IReloaderModule>(module => module.IsReloading);

    /// <summary>
    /// Whether or not the firearm is being unloaded.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is being unloaded</returns>
    public static bool IsUnloading(this Firearm firearm)
        => firearm.GetProperty<bool, IReloaderModule>(module => module.IsUnloading);

    /// <summary>
    /// Whether or not the firearm is being unloaded or reloaded.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is being unloaded or reloaded</returns>
    public static bool IsUnloadingOrReloading(this Firearm firearm)
        => firearm.GetProperty<bool, IReloaderModule>(module => module.IsReloadingOrUnloading);
    
    /// <summary>
    /// Whether or not the firearm is automatic.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is automatic</returns>
    public static bool IsAutomatic(this Firearm firearm)
        => firearm.HasModule<AutomaticActionModule>();

    /// <summary>
    /// Whether or not the firearm is currently being aimed.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm is being aimed</returns>
    public static bool IsAiming(this Firearm firearm)
        => firearm.GetProperty<bool, LinearAdsModule>(module => module._userInput);

    /// <summary>
    /// Whether or not the firearm can be aimed.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm can be aimed</returns>
    public static bool IsAimingAllowed(this Firearm firearm)
        => !firearm.IsAiming() && (!firearm.TryGetModule<IAdsPreventerModule>(out var module) || module.AdsAllowed);
    
    /// <summary>
    /// Whether or not the firearm has it's flashlight enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm has it's flashlight enabled</returns>
    public static bool IsFlashlightEnabled(this Firearm firearm)
        => firearm.IsEmittingLight;

    /// <summary>
    /// Whether or not the firearm has it's night vision enabled.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>true if the firearm has it's night vision enabled</returns>
    public static bool IsNightVisionEnabled(this Firearm firearm)
        => firearm.IsAiming() && firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.NightVision);

    /// <summary>
    /// Gets the firearm's inaccuracy while aiming.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>the firearm's aiming inaccuracy</returns>
    public static float GetAimInaccuracy(this Firearm firearm)
        => firearm.GetProperty<float, LinearAdsModule>(module => module.Inaccuracy);
    
    /// <summary>
    /// Gets the firearm's fire rate.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>the firearm's firing rate</returns>
    public static float GetFireRate(this Firearm firearm)
        => firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule)
            ? automaticActionModule.BaseFireRate
            : 0f;
    
    /// <summary>
    /// Gets the firearm's ammo type.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The firearm's ammunition type</returns>
    public static ItemType GetAmmoType(this Firearm firearm)
        => firearm.GetMagazineModule().AmmoType;

    /// <summary>
    /// Sets the firearm's firing rate.
    /// <remarks>Only works on automatic firearms.</remarks>
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="fireRate">The new firing rate.</param>
    /// <returns>true if the firing rate was succesfully changed (false for non-automatic firearms)</returns>
    public static bool SetFireRate(this Firearm firearm, float fireRate)
    {
        if (!firearm.TryGetModule<AutomaticActionModule>(out var automaticActionModule))
            return false;

        automaticActionModule.BaseFireRate = fireRate;
        return true;
    }

    /// <summary>
    /// Gets the amount of ammo stored in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The amount of stored ammo.</returns>
    public static int GetStoredAmmo(this Firearm firearm)
    {
        if (firearm is null)
            return 0;

        var stored = 0;
        
        for (var i = 0; i < firearm._modules.Length; i++)
        {
            if (firearm._modules[i] is not IAmmoContainerModule ammoContainerModule)
                continue;

            stored += ammoContainerModule.AmmoStored;
        }

        return stored;
    }

    /// <summary>
    /// Gets the maximum amount of ammo which can be stored in a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The maximum amount of stored ammo.</returns>
    public static int GetMaxAmmo(this Firearm firearm)
    {
        if (firearm is null)
            return 0;

        var max = 0;
        
        for (var i = 0; i < firearm._modules.Length; i++)
        {
            if (firearm._modules[i] is not IAmmoContainerModule ammoContainerModule)
                continue;

            max += ammoContainerModule.AmmoMax;
        }

        return max;
    }
    
    /// <summary>
    /// Sets the firearm's stored ammo.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="ammo">The new amount of stored ammo.</param>
    public static void SetStoredAmmo(this Firearm firearm, int ammo)
        => firearm.GetMagazineModule()!.AmmoStored = ammo;

    /// <summary>
    /// Sets the firearm's max stored ammo capacity.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="ammo">The new max stored ammo capacity.</param>
    public static void SetMaxAmmo(this Firearm firearm, int ammo)
        => firearm.GetMagazineModule()!._defaultCapacity = ammo;
    
    /// <summary>
    /// Gets the firearm's damage at a specific distance.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="distance">The target distance.</param>
    /// <returns>The damage to be dealt at the specified distance from target.</returns>
    public static float GetDamageAtDistance(this Firearm firearm, float distance)
        => firearm.TryGetModule<HitscanHitregModuleBase>(out var hitregModule) 
            ? hitregModule.DamageAtDistance(distance) 
            : 0f;

    // Can you tell that I have no idea what "DamageFalloffDistance" is?
    /// <summary>
    /// Gets the distance damage falloff.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The distance damage falloff.</returns>
    public static float GetDamageFalloffDistance(this Firearm firearm)
        => firearm.TryGetModule<HitscanHitregModuleBase>(out var hitregModule) 
            ? hitregModule.DamageFalloffDistance 
            : 0f;

    /// <summary>
    /// Gets the firearm's recoil settings.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The firearm's recoil settings.</returns>
    public static RecoilSettings GetRecoilSettings(this Firearm firearm)
        => firearm.TryGetModule<RecoilPatternModule>(out var recoilPatternModule)
            ? recoilPatternModule.BaseRecoil
            : default;

    /// <summary>
    /// Gets the firearm's buckshot pattern settings.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The firearm's buckshot pattern settings.</returns>
    public static BuckshotSettings GetBuckshotSettings(this Firearm firearm)
        => firearm.TryGetModule<BuckshotHitreg>(out var buckshotHitreg) 
            ? buckshotHitreg.BasePattern 
            : default;

    /// <summary>
    /// Sets the firearm's recoil settings.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="settings">The new recoil settings.</param>
    /// <returns>true if the settings were modified (false for firearms without a recoil pattern)</returns>
    public static bool SetRecoilSettings(this Firearm firearm, RecoilSettings settings)
    {
        if (!firearm.TryGetModule<RecoilPatternModule>(out var recoilPatternModule))
            return false;

        recoilPatternModule.BaseRecoil = settings;
        return true;
    }

    /// <summary>
    /// Sets the firearm's buckshot pattern settings.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="settings">The new buckshot pattern settings.</param>
    /// <returns>true if the settings were modified (false for firearms that are not shotguns)</returns>
    public static bool SetBuckshotSettings(this Firearm firearm, BuckshotSettings settings)
    {
        if (!firearm.TryGetModule<BuckshotHitreg>(out var buckshotHitreg))
            return false;

        buckshotHitreg.BasePattern = settings;
        return true;
    }
}