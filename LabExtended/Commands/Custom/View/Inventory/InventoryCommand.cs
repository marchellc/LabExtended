using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;

using LabExtended.API;

using LabExtended.Extensions;
using LabExtended.Commands.Attributes;
using LabExtended.Utilities.Firearms;

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    /// <summary>
    /// Lists all items and ammo in a player's inventory.
    /// </summary>
    [CommandOverload("inv", "Lists all items in a player's inventory.", null)]
    public void Inventory(
        [CommandParameter("Target", "The targeted player (defaults to you).")] ExPlayer? target = null)
    {
        target ??= Sender;

        if (target.Inventory.ItemCount < 1 && !target.Ammo.HasAnyAmmo)
        {
            Fail($"Inventory of player {target.ToCommandString()} is empty.");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine($"Listing inventory of player {target.ToCommandString()}:");

            if (target.Inventory.ItemCount > 0)
            {
                x.AppendLine("- Items");

                foreach (var item in target.Inventory.Items)
                {
                    x.AppendLine($"  > ({item.GetInventorySlot()}) [{item.ItemSerial}] {item.ItemTypeId}");

                    if (target.Inventory.CurrentItem != null && target.Inventory.CurrentItem == item)
                    {
                        x.AppendLine("    - Held Item");
                    }
                    
                    if (item is Firearm firearm)
                    {
                        if (firearm.TryGetModule<IPrimaryAmmoContainerModule>(out var ammoContainer))
                        {
                            x.AppendLine($"    - Firearm Ammo: {ammoContainer!.AmmoStored} / {ammoContainer.AmmoMax} ({ammoContainer.AmmoType})");
                        }
                    }

                    if (item is ILightEmittingItem lightEmittingItem)
                    {
                        x.AppendLine($"    - Emitting Light: {lightEmittingItem.IsEmittingLight}");
                    }
                }
            }

            if (target.Ammo.HasAnyAmmo)
            {
                x.AppendLine("- Ammo");

                foreach (var pair in target.Ammo.Ammo)
                {
                    x.AppendLine($"  > {pair.Key}: {pair.Value}");
                }
            }

            if (target.Ammo.CustomAmmo.Count > 0)
            {
                x.AppendLine("- Custom Ammo");

                foreach (var pair in target.Ammo.CustomAmmo)
                {
                    x.AppendLine($"  > {pair.Key}: {pair.Value}");
                }
            }
        });
    }
}