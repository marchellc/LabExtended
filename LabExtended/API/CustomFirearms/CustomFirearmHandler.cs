using LabExtended.API.CustomFirearms.Behaviours;
using LabExtended.API.CustomFirearms.Properties;

using LabExtended.API.CustomItems;
using LabExtended.API.CustomItems.Behaviours;
using LabExtended.Attributes;
using LabExtended.Utilities.Firearms;

#pragma warning disable CS8603 // Possible null reference return.

namespace LabExtended.API.CustomFirearms;

/// <summary>
/// Manages a Custom Firearm item.
/// </summary>
[LoaderIgnore]
public abstract class CustomFirearmHandler : CustomItemHandler
{
    /// <summary>
    /// Gets the firearm's inventory properties.
    /// </summary>
    public CustomFirearmInventoryProperties FirearmInventoryProperties => InventoryProperties as CustomFirearmInventoryProperties;
    
    /// <summary>
    /// Gets the firearm's pickup properties.
    /// </summary>
    public CustomFirearmPickupProperties FirearmPickupProperties => PickupProperties as CustomFirearmPickupProperties;

    /// <summary>
    /// Whether or not this firearm uses custom ammo.
    /// </summary>
    public bool UsesCustomAmmo =>
        FirearmInventoryProperties.AmmoType is ItemType.None && FirearmInventoryProperties.AmmoId != null;

    internal override void InternalInitializeItem(CustomItemInventoryBehaviour item, CustomItemPickupBehaviour? pickup)
    {
        base.InternalInitializeItem(item, pickup);

        if (item is CustomFirearmInventoryBehaviour firearmInventoryBehaviour)
        {
            firearmInventoryBehaviour.Modules = firearmInventoryBehaviour.Item.GetModules();

            if (FirearmInventoryProperties.Attachments != null)
            {
                firearmInventoryBehaviour.Item.SetAttachments(attachment =>
                    FirearmInventoryProperties.Attachments.DefaultAttachments.Contains(attachment.Name));
            }
        }
    }
}