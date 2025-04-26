using InventorySystem.Items.Keycards;

using Mirror;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard parameter with a singular string property.
/// </summary>
public class SingleStringProperty : KeycardValue
{
    private Action<SingleStringProperty, KeycardItem>? applyAction;
    
    /// <summary>
    /// Creates a new <see cref="SingleStringProperty"/> instance.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="applyAction">Delegate used to apply the custom value.</param>
    public SingleStringProperty(Type type, Action<SingleStringProperty, KeycardItem>? applyAction = null) : base(type)
        => this.applyAction = applyAction;

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
    
    /// <inheritdoc cref="KeycardValue.Apply"/>
    public override void Apply(KeycardItem item)
    {
        base.Apply(item);
        
        applyAction?.Invoke(this, item);
    }
}