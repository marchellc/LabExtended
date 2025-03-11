using UnityEngine;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Used to configure custom items.
/// </summary>
public class CustomItemBuilder
{
    /// <summary>
    /// Gets the custom item data.
    /// </summary>
    public virtual CustomItemData Data { get; } = new();

    /// <summary>
    /// Sets the item's name.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomItemBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        
        Data.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the item's description.
    /// </summary>
    /// <param name="description">Description.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomItemBuilder WithDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));
        
        Data.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the item's inventory type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>This builder instance.</returns>
    public CustomItemBuilder WithInventoryType(ItemType type)
    {
        Data.InventoryType = type;
        return this;
    }

    /// <summary>
    /// Sets the item's pickup type.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>This builder instance.</returns>
    public CustomItemBuilder WithPickupType(ItemType type)
    {
        Data.PickupType = type;
        return this;
    }

    /// <summary>
    /// Sets the item's pickup scale.
    /// </summary>
    /// <param name="scale">The pickup scale.</param>
    /// <returns>This builder instance.</returns>
    public CustomItemBuilder WithPickupScale(Vector3 scale)
    {
        Data.PickupScale = scale;
        return this;
    }

    /// <summary>
    /// Sets the item's class.
    /// </summary>
    /// <param name="type">The class.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomItemBuilder WithType(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        Data.Type = type;
        return this;
    }
    
    /// <summary>
    /// Sets the item's class.
    /// </summary>
    /// <typeparam name="T">The class.</typeparam>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CustomItemBuilder WithType<T>() where T : CustomItemInstance
    {
        Data.Type = typeof(T);
        return this;
    }
}