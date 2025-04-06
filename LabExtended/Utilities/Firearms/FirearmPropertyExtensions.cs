using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

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
    /// Gets the firearm's inaccuracy while aiming.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>the firearm's aiming inaccuracy</returns>
    public static float GetAimInaccuracy(this Firearm firearm)
        => firearm.GetProperty<float, LinearAdsModule>(module => module.Inaccuracy);

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
}