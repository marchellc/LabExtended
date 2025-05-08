using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular byte property.
/// </summary>
public class SingleByteProperty : KeycardValue
{
    /// <summary>
    /// Creates a new <see cref="SingleByteProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    public SingleByteProperty(Type type) : base(type) { }

    /// <summary>
    /// Gets or sets the byte value of this property.
    /// </summary>
    public byte Value { get; set; }

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
        => writer.WriteByte(Value);

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
    {
        Value = 0;
    }
}