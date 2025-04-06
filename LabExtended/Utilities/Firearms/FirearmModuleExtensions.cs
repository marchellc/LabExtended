using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

namespace LabExtended.Utilities.Firearms;

/// <summary>
/// Extensions used to interact with firearm modules.
/// </summary>
public static class FirearmModuleExtensions
{
    /// <summary>
    /// Retrieves a property from a given module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="getter">The property getter.</param>
    /// <param name="property">The resulting property value (or null if false is returned)</param>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <typeparam name="TModule">The target module type.</typeparam>
    /// <returns>true if the module is found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetProperty<TProperty, TModule>(this Firearm firearm, Func<TModule?, TProperty> getter,
        out TProperty? property)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (getter is null)
            throw new ArgumentNullException(nameof(getter));

        if (!firearm.TryGetModule<TModule>(out var module))
        {
            property = default;
            return false;
        }

        property = getter(module);
        return true;
    }
    
    /// <summary>
    /// Retrieves a property from a given module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="getter">The property getter.</param>
    /// <param name="defaultValue">The default property value.</param>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <typeparam name="TModule">The target module type.</typeparam>
    /// <returns>Value returned by <see cref="getter"/> if module is found, otherwise <see cref="defaultValue"/></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static TProperty? GetProperty<TProperty, TModule>(this Firearm firearm, Func<TModule?, TProperty> getter, 
        TProperty? defaultValue = default)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (getter is null)
            throw new ArgumentNullException(nameof(getter));

        if (!firearm.TryGetModule<TModule>(out var module))
            return defaultValue;

        return getter(module);
    }

    /// <summary>
    /// Invokes the specified delegate on a firearm's module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="action">The action to perform.</param>
    /// <typeparam name="TModule">The module type.</typeparam>
    /// <returns>true if the module was found and the delegate invoked</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool DoModule<TModule>(this Firearm firearm, Func<TModule?, bool> action)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (!firearm.TryGetModule<TModule>(out var module))
            return false;

        return action(module);
    }
    
    /// <summary>
    /// Gets the firearm's aiming module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The found module instance</returns>
    public static LinearAdsModule? GetAdsModule(this Firearm firearm)
        => TryGetModule<LinearAdsModule>(firearm, out var adsModule) ? adsModule : null;
    
    /// <summary>
    /// Gets the firearm's magazine module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <returns>The found magazine module instance</returns>
    public static MagazineModule? GetMagazineModule(this Firearm firearm)
        => TryGetModule<MagazineModule>(firearm, out var module) ? module : null;

    /// <summary>
    /// Whether or not a firearm has a specific module.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <typeparam name="T">The target module type.</typeparam>
    /// <returns>true if the firearm has the module</returns>
    public static bool HasModule<T>(this Firearm firearm)
        => firearm != null && TryGetModule<T>(firearm, out _);

    /// <summary>
    /// Gets the module instance from a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <typeparam name="T">The target module type.</typeparam>
    /// <returns>Found module instance (or null)</returns>
    public static T? GetModule<T>(this Firearm firearm)
        => TryGetModule<T>(firearm, out var module) ? module : default;

    /// <summary>
    /// Gets the module instance from a firearm.
    /// </summary>
    /// <param name="firearm">The target firearm.</param>
    /// <param name="module">Found module instance.</param>
    /// <typeparam name="T">The target module type.</typeparam>
    /// <returns>true if the module was found</returns>
    public static bool TryGetModule<T>(this Firearm firearm, out T? module)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        module = default;

        for (var i = 0; i < firearm.Modules.Length; i++)
        {
            var curModule = firearm.Modules[i];

            if (curModule is not T targetModule)
                continue;

            module = targetModule;
            return true;
        }

        return false;
    }
}