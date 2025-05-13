namespace LabExtended.API.CustomItems.Properties;

/// <summary>
/// Gets the properties of a custom item while in inventory.
/// </summary>
public class CustomItemInventoryProperties : CustomItemCommonProperties
{
    /// <summary>
    /// Whether or not the item should be dropped when it's owner leaves.
    /// <remarks>Item will be destroyed if the owner leaves and this is set to false.</remarks>
    /// </summary>
    public bool DropOnOwnerLeave { get; set; }
}