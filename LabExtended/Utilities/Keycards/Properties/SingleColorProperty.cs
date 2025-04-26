using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular color property.
/// </summary>
public class SingleColorProperty : KeycardValue
{
    private Action<SingleColorProperty, KeycardItem>? applyAction;
    
    /// <summary>
    /// Creates a new <see cref="SingleColorProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="applyAction">Delegate used to apply the custom value.</param>
    public SingleColorProperty(Type type, Action<SingleColorProperty, KeycardItem>? applyAction = null) : base(type)
        => this.applyAction = applyAction;

    /// <summary>
    /// Gets or sets the color value of this property.
    /// </summary>
    public Color32 Value { get; set; }

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
        => writer.WriteColor32(Value);

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
    {
        Value = default;
    }

    /// <inheritdoc cref="KeycardValue.Apply"/>
    public override void Apply(KeycardItem item)
    {
        base.Apply(item);
        
        applyAction?.Invoke(this, item);
    }
}