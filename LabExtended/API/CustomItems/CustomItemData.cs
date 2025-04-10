using UnityEngine;

namespace LabExtended.API.CustomItems;

/// <summary>
/// Used to store information about registered custom items.
/// </summary>
public class CustomItemData
{
    /// <summary>
    /// Gets the item's name.
    /// </summary>
    public string? Name { get; internal set; }
    
    /// <summary>
    /// Gets the item's description.
    /// </summary>
    public string? Description { get; internal set; }
    
    /// <summary>
    /// Gets the item's custom weight.
    /// </summary>
    public float? Weight { get; internal set; }
    
    /// <summary>
    /// Gets the item's pickup type.
    /// </summary>
    public ItemType PickupType { get; internal set; } = ItemType.None;
    
    /// <summary>
    /// Gets the item's inventory type.
    /// </summary>
    public ItemType InventoryType { get; internal set; } = ItemType.None;
    
    /// <summary>
    /// Gets the item's pickup scale.
    /// </summary>
    public Vector3? PickupScale { get; internal set; }
    
    /// <summary>
    /// Gets the item's class.
    /// </summary>
    public Type? Type { get; internal set; }
}