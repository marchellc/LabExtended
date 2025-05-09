using LabExtended.API.Scp914.Interfaces;

namespace LabExtended.API.Scp914.Outputs;

/// <summary>
/// Defines an item output.
/// </summary>
public readonly struct Scp914ItemOutput : IScp914Output
{
    /// <summary>
    /// Gets the chance for this item to be upgraded to.
    /// </summary>
    public float Chance { get; }
    
    /// <summary>
    /// Gets the output item type.
    /// </summary>
    public ItemType Item { get; }

    /// <summary>
    /// Creates a new <see cref="Scp914ItemOutput"/> entry.
    /// </summary>
    /// <param name="chance">The entry chance.</param>
    /// <param name="item">The output item.</param>
    public Scp914ItemOutput(float chance, ItemType item)
    {
        Chance = chance;
        Item = item;
    }
}