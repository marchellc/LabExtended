using InventorySystem.Items.Firearms;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Attachments;

/// <summary>
/// Views and modifies attachments.
/// </summary>
[Command("attachments", "Views and modifies firearm attachments.")]
public class AttachmentsCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Lists all available attachments on a firearm.
    /// </summary>
    [CommandOverload("item", "Shows all attachments on an item.")]
    public void Item(
        [CommandParameter("Type", "The type of the firearm.")] ItemType type)
    {
        if (!type.TryGetItemPrefab<Firearm>(out var firearm))
        {
            Fail($"Could not get a firearm prefab for item '{type}'");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine($"Firearm '{firearm.ItemTypeId}' ({firearm.Attachments?.Length ?? -1} attachments):");

            if (firearm.Attachments != null)
            {
                for (var i = 0; i < firearm.Attachments.Length; i++)
                {
                    var attachment = firearm.Attachments[i];

                    attachment.GetNameAndDescription(out var name, out var description);
                    
                    x.AppendLine($"- {attachment.Name}");
                    x.AppendLine($"  <- Name: {name}");
                    x.AppendLine($"  <- Description: {description}");
                    x.AppendLine($"  <- Slot: {attachment.Slot}");
                    x.AppendLine($"  <- Downsides: {attachment.DescriptiveCons}");
                    x.AppendLine($"  <- Advantages: {attachment.DescriptivePros}");
                    x.AppendLine($"  <- Weight: {attachment.Weight} kg");
                    x.AppendLine($"  <- Index: {attachment.Index} ({i})");
                    x.AppendLine($"  <- Length: {attachment.Length} m");

                    if (attachment._parameterValues != null && attachment._parameterStates != null)
                    {
                        for (var y = 0; y < attachment._parameterStates.Length; y++)
                        {
                            if (y >= attachment._parameterValues.Length)
                                continue;

                            var state = attachment._parameterStates[y];
                            var value = attachment._parameterValues[y];

                            x.AppendLine($"  <- [{y}] Parameter '{state}': {value}");
                        }
                    }
                }
            }
        });
    }
}