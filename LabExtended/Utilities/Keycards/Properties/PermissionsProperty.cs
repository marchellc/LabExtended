using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Keycards;

using Mirror;

using UnityEngine;

namespace LabExtended.Utilities.Keycards.Properties;

/// <summary>
/// A keycard permissions property.
/// </summary>
public class PermissionsProperty : KeycardValue
{
    private KeycardLevels levels = new(DoorPermissionFlags.None);
    
    /// <summary>
    /// Creates a new <see cref="PermissionsProperty"/> instance.
    /// </summary>
    public PermissionsProperty() : base(typeof(CustomPermsDetail)) { }

    /// <summary>
    /// Gets or sets the custom permissions.
    /// </summary>
    public DoorPermissionFlags Flags
    {
        get => levels.Permissions;
        set => levels = new(value);
    }
    
    /// <summary>
    /// Gets or sets the color value of this property.
    /// </summary>
    public Color32? Color { get; set; }
    
    /// <summary>
    /// Whether or not the <see cref="Flags"/> has the specified flag.
    /// </summary>
    /// <param name="flag">The flag to check for.</param>
    /// <returns>true if the flag was found.</returns>
    public bool HasFlag(DoorPermissionFlags flag)
        => Flags.HasFlag(flag);

    /// <summary>
    /// Whether or not the <see cref="Flags"/> has any of the specified flags.
    /// </summary>
    /// <param name="flag">The flags to check for.</param>
    /// <returns>true if any flags were found</returns>
    public bool HasAnyFlags(DoorPermissionFlags flag)
        => Flags.HasFlagAny(flag);
    
    /// <summary>
    /// Whether or not the <see cref="Flags"/> has all of the specified flags.
    /// </summary>
    /// <param name="flag">The flags to check for.</param>
    /// <returns>true if all flags were found</returns>
    public bool HasAllFlags(DoorPermissionFlags flag)
        => Flags.HasFlagAll(flag);
    
    /// <summary>
    /// Adds a permission flag.
    /// </summary>
    /// <param name="flag">The flag to add.</param>
    public void AddFlag(DoorPermissionFlags flag)
        => Flags |= flag;
    
    /// <summary>
    /// Removes a permission flag.
    /// </summary>
    /// <param name="flag">The flag to remove.</param>
    public void RemoveFlag(DoorPermissionFlags flag)
        => Flags &= ~flag;

    /// <inheritdoc cref="KeycardValue.Write"/>
    public override void Write(NetworkWriter writer, KeycardItem item)
    {
        writer.WriteUShort((ushort)levels.Permissions);
        writer.WriteColor32Nullable(Color);
    }

    /// <inheritdoc cref="KeycardValue.Reset"/>
    public override void Reset()
    {
        Flags = DoorPermissionFlags.None;
        Color = null;
    }

    /// <inheritdoc cref="KeycardValue.Apply"/>
    public override void Apply(KeycardItem item)
    {
        base.Apply(item);

        if (item != null)
            CustomPermsDetail.ServerCustomPermissions[item.ItemSerial] = levels.Permissions;
    }
}