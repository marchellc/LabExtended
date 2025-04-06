using InventorySystem.Items.Firearms;

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
    public static bool TryGetProperty<TProperty, TModule>(this Firearm firearm, Func<TModule, TProperty> getter,
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
    public static TProperty? GetProperty<TProperty, TModule>(this Firearm firearm, Func<TModule, TProperty> getter, 
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
    public static bool DoModule<TModule>(this Firearm firearm, Func<TModule, bool> action)
    {
        if (firearm is null)
            throw new ArgumentNullException(nameof(firearm));

        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (!firearm.TryGetModule<TModule>(out var module))
            return false;

        return action(module);
    }
}