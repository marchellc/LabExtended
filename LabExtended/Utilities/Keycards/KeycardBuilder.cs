using Interactables.Interobjects.DoorUtils;

using InventorySystem.Items.Keycards;

using LabExtended.Core.Pooling;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Extensions;
using LabExtended.Utilities.Keycards.Properties;

using Mirror;

using NorthwoodLib.Pools;

using UnityEngine;

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
    private bool propertiesOrdered;

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
    /// Sets the keycard's name.
    /// </summary>
    /// <param name="name">The name to set.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithName(string name)
    {
        Name.Value = name;
        return this;
    }

    /// <summary>
    /// Sets the keycard's serial number.
    /// </summary>
    /// <param name="serialNumber">The serial number to set.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithSerialNumber(string serialNumber)
    {
        SerialNumber.Value = serialNumber;
        return this;
    }

    /// <summary>
    /// Sets the keycard's label text and color.
    /// </summary>
    /// <param name="label">The text of the label.</param>
    /// <param name="color">The color of the label.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithLabel(string label, Color32? color = null)
    {
        Label.Value = label;
        
        if (color.HasValue)
            Label.Color = color.Value;
        
        return this;
    }

    /// <summary>
    /// Sets the keycard's tint color.
    /// </summary>
    /// <param name="color">The color to set.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithTint(Color32 color)
    {
        Tint.Value = color;
        return this;
    }

    /// <summary>
    /// Sets the keycard's wear level.
    /// </summary>
    /// <param name="level">The level to set.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithWearLevel(byte level)
    {
        Wear.Value = level;
        return this;
    }

    /// <summary>
    /// Sets the keycard's permissions.
    /// </summary>
    /// <param name="permissions">The permission flags to set.</param>
    /// <param name="color">The color of the permission chart.</param>
    /// <returns>This builder instance.</returns>
    public KeycardBuilder WithPermissions(DoorPermissionFlags permissions, Color32? color = null)
    {
        Permissions.Flags = permissions;
        
        if (color.HasValue)
            Permissions.Color = color.Value;
        
        return this;
    }
    
    /// <summary>
    /// Applies the specified properties server-side.
    /// </summary>
    /// <param name="item">The target item.</param>
    public void Apply(KeycardItem item, ushort itemSerial)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));
        
        properties.ForEach(p => p.Apply(item, itemSerial));
    }

    /// <summary>
    /// Serializes the custom properties of this keycard to a byte array segment.
    /// </summary>
    /// <param name="item">The targeted keycard item.</param>
    /// <returns>The serialized byte array payload.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public ArraySegment<byte> ToSegment(KeycardItem item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (!propertiesOrdered)
        {
            var temp = ListPool<KeycardValue>.Shared.Rent(properties.Count);
            
            temp.AddRange(properties.OrderBy(x => item.Details.FindIndex(d => d.GetType() == x.DetailType)));
            
            properties.Clear();
            properties.AddRange(temp);
            
            ListPool<KeycardValue>.Shared.Return(temp);
            
            propertiesOrdered = true;
        }
        
        using (var writer = NetworkWriterPool.Get())
        {
            properties.ForEach(v => v.Write(writer, item));
            return writer.ToArraySegment();
        }
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