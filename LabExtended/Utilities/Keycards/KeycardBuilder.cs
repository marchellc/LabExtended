using InventorySystem.Items.Keycards;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;
using LabExtended.Utilities.Keycards.Properties;

using Mirror;

using NorthwoodLib.Pools;

namespace LabExtended.Utilities.Keycards;

/// <summary>
/// Used to build custom keycard data.
/// </summary>
public class KeycardBuilder : PoolObject
{
    /// <summary>
    /// Gets a pooled builder instance.
    /// </summary>
    public static KeycardBuilder Pooled => ObjectPool<KeycardBuilder>.Shared.Rent(null, () => new());
        
    private KeycardBuilder() { }
    private List<KeycardValue> properties = new();

    /// <summary>
    /// Gets the keycard's custom item name property.
    /// </summary>
    public SingleStringProperty Name { get; } = new(typeof(CustomItemNameDetail));

    /// <summary>
    /// Gets the keycard's custom serial number.
    /// </summary>
    public SingleStringProperty SerialNumber { get; } = new(typeof(CustomSerialNumberDetail));

    /// <summary>
    /// Gets the keycard's custom label property.
    /// </summary>
    public SingleStringAndColorProperty Label { get; } = new(typeof(CustomLabelDetail));
    
    /// <summary>
    /// Gets the keycard's custom tint property.
    /// </summary>
    public SingleColorProperty Tint { get; } = new(typeof(CustomTintDetail));

    /// <summary>
    /// Gets the keycard's custom wear level property.
    /// </summary>
    public SingleByteProperty Wear { get; } = new(typeof(CustomWearDetail));

    /// <summary>
    /// Gets the keycard's custom permissions property.
    /// </summary>
    public PermissionsProperty Permissions { get; } = new();
    
    /// <summary>
    /// Gets the keycard's custom rank property.
    /// </summary>
    public RankProperty Rank { get; } = new();

    /// <summary>
    /// Applies the specified properties server-side.
    /// </summary>
    /// <param name="item">The target item.</param>
    public void Apply(KeycardItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        
        properties.ForEach(p => p.Apply(item));
    }

    /// <summary>
    /// Serializes the custom properties of this keycard to a byte array segment.
    /// </summary>
    /// <param name="item">The targeted keycard item.</param>
    /// <returns>The serialized byte array payload.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public ArraySegment<byte> ToSegment(KeycardItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        var temp = ListPool<KeycardValue>.Shared.Rent(properties.Count);
        var data = default(ArraySegment<byte>);

        using (var writer = NetworkWriterPool.Get())
        {
            temp.AddRange(properties.OrderBy(x => item.Details.FindIndex(d => d.GetType() == x.DetailType)));
            temp.ForEach(v => v.Write(writer, item));

            data = writer.ToArraySegment();
        }

        ListPool<KeycardValue>.Shared.Return(temp);
        return data;
    }

    /// <inheritdoc cref="PoolObject.OnReturned"/>
    public override void OnReturned()
    {
        base.OnReturned();
        
        properties.ForEach(p => p.Reset());
    }

    /// <inheritdoc cref="PoolObject.OnConstructed"/>
    public override void OnConstructed()
    {
        base.OnConstructed();
        
        properties.Add(Name);
        properties.Add(Label);
        properties.Add(Permissions);
        properties.Add(Rank);
        properties.Add(SerialNumber);
        properties.Add(Tint);
        properties.Add(Wear);
    }
}