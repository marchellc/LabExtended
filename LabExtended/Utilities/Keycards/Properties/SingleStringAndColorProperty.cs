using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular string and color property.
/// </summary>
public class SingleStringAndColorProperty : KeycardValue
{
    /// <summary>
    /// Creates a new <see cref="SingleStringAndColorProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    public SingleStringAndColorProperty(Type type) : base(type) { }
    
    /// <summary>
    /// Gets or sets the string value of this property.
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Gets or sets the color value of this property.
    /// </summary>
    public Color32 Color { get; set; }

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
    {
        writer.WriteString(Value);
        writer.WriteColor32(Color);
    }

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
    {
        Value = null;
        Color = default;
    }
}