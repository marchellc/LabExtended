using LabExtended.API.Scp914.Interfaces;

namespace LabExtended.API.Scp914.Outputs;

/// <summary>
/// Defines an ammo output.
/// </summary>
public readonly struct Scp914AmmoOutput : IScp914Output
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
    /// Gets the amount of ammo to add.
    /// </summary>
    public ushort Amount { get; }

    /// <summary>
    /// Creates a new <see cref="Scp914AmmoOutput"/> entry.
    /// </summary>
    /// <param name="chance">The entry chance.</param>
    /// <param name="item">The output item.</param>
    /// <param name="amount">The amount of ammo to add.</param>
    public Scp914AmmoOutput(float chance, ItemType item, ushort amount)
    {
        Chance = chance;
        Item = item;
        Amount = amount;
    }
}