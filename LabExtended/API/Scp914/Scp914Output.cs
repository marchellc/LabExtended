namespace LabExtended.API.Scp914;

/// <summary>
/// Defines the output of a SCP-914 recipe.
/// </summary>
public readonly struct Scp914Output
{
    /// <summary>
    /// Gets the chance for this item to be upgraded to.
    /// </summary>
    public readonly float Chance;
    
    /// <summary>
    /// Gets the output item type.
    /// </summary>
    public readonly ItemType Item;

    /// <summary>
    /// Creates a new <see cref="Scp914Output"/> entry.
    /// </summary>
    /// <param name="chance">The entry chance.</param>
    /// <param name="item">The output item.</param>
    public Scp914Output(float chance, ItemType item)
    {
        Chance = chance;
        Item = item;
    }
}