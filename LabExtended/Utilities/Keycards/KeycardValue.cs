using InventorySystem.Items.Keycards;
using Mirror;

namespace LabExtended.Utilities.Keycards;

/// <summary>
/// Represents the custom value of a keycard property.
/// </summary>
public abstract class KeycardValue
{
    /// <summary>
    /// Creates a new <see cref="KeycardValue"/> instance.
    /// </summary>
    /// <param name="type"></param>
    public KeycardValue(Type type)
    {
        DetailType = type;
    }
    
    /// <summary>
    /// Gets the detail type.
    /// </summary>
    public Type DetailType { get; }
    
    /// <summary>
    /// Writes the value of this property.
    /// </summary>
    /// <param name="writer">The target writer.</param>
    /// <param name="item">The target keycard.</param>
    public abstract void Write(NetworkWriter writer, KeycardItem item);

    /// <summary>
    /// Resets the value of this property.
    /// </summary>
    public abstract void Reset();

    /// <summary>
    /// Applies the value of this property on a specific item.
    /// </summary>
    /// <param name="item">The target item.</param>
    public virtual void Apply(KeycardItem item) { }
}