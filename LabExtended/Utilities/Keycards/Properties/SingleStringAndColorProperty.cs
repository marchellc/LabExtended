using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular string and color property.
/// </summary>
public class SingleStringAndColorProperty : KeycardValue
{
    private Action<SingleStringAndColorProperty, KeycardItem>? applyAction;
    
    /// <summary>
    /// Creates a new <see cref="SingleStringAndColorProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="applyAction">Delegate used to apply the custom value.</param>
    public SingleStringAndColorProperty(Type type, Action<SingleStringAndColorProperty, KeycardItem>? applyAction = null) : base(type)
        => this.applyAction = applyAction;
    
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

    /// <inheritdoc cref="KeycardValue.Apply"/>
    public override void Apply(KeycardItem item)
    {
        base.Apply(item);
        
        applyAction?.Invoke(this, item);
    }
}