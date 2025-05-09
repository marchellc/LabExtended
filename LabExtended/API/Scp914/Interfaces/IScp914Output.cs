namespace LabExtended.API.Scp914.Interfaces;

/// <summary>
/// Represents a possible output of a SCP-914 recipe.
/// </summary>
public interface IScp914Output
{
    /// <summary>
    /// Gets the output's chance.
    /// </summary>
    public float Chance { get; }

    /// <summary>
    /// Gets the item's type.
    /// </summary>
    public ItemType Item { get; }
}