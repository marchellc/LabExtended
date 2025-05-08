using InventorySystem.Items.Keycards;

using Mirror;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular string property.
/// </summary>
public class SingleStringProperty : KeycardValue
{
    /// <summary>
    /// Creates a new <see cref="SingleStringProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    public SingleStringProperty(Type type) : base(type) { }

    /// <summary>
    /// Gets or sets the value of this property.
    /// </summary>
    public string? Value { get; set; }

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
        => writer.Write(Value);

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
        => Value = null;
}