using InventorySystem;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;

using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;
using LabExtended.Utilities.Firearms;

namespace LabExtended.Commands.Custom.Attachments;

/// <summary>
/// Views and modifies attachments.
/// </summary>
[Command("attachments", "Views and modifies firearm attachments.")]
public class AttachmentsCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Randomizes all attachments.
    /// </summary>
    [CommandOverload("random", "Randomizes all attachments of a firearm.", null)]
    public void Random(ushort firearmSerial)
    {
        if (!InventoryExtensions.ServerTryGetItemWithSerial(firearmSerial, out var item)
            || item is not Firearm firearm || firearm == null)
        {
            Fail($"Could not find an active firearm with serial '{firearmSerial}'");
            return;
        }
        
        firearm.SetRandomAttachments();
        
        Ok($"Successfully modified attachments of firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
    }
    
    /// <summary>
    /// Clears all attachments.
    /// </summary>
    [CommandOverload("clear", "Clears all attachments on a firearm.", null)]
    public void Clear(ushort firearmSerial)
    {
        if (!InventoryExtensions.ServerTryGetItemWithSerial(firearmSerial, out var item)
            || item is not Firearm firearm || firearm == null)
        {
            Fail($"Could not find an active firearm with serial '{firearmSerial}'");
            return;
        }

        if (firearm.SetAttachments(_ => false))
        {
            Ok($"Successfully modified attachments of firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
            return;
        }
        
        Fail($"No modifications were made to firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
    }
    
    /// <summary>
    /// Disables selected attachments.
    /// </summary>
    [CommandOverload("disable", "Disables a list of attachments on a firearm.", null)]
    public void Disable(
        [CommandParameter(Name = "Serial", Description = "The serial number of the firearm.")]
        ushort firearmSerial,

        [CommandParameter(Name = "Attachments", Description = "List of attachments to disable.")]
        List<AttachmentName> attachments)
    {
        if (!InventoryExtensions.ServerTryGetItemWithSerial(firearmSerial, out var item)
            || item is not Firearm firearm || firearm == null)
        {
            Fail($"Could not find an active firearm with serial '{firearmSerial}'");
            return;
        }

        if (firearm.DisableAttachments(attachments))
        {
            Ok($"Successfully modified attachments of firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
            return;
        }
        
        Fail($"No modifications were made to firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
    }
    
    /// <summary>
    /// Enables selected attachments.
    /// </summary>
    [CommandOverload("enable", "Enables a list of attachments on a firearm.", null)]
    public void Enable(
        [CommandParameter("Serial", "The serial of the target firearm.")] ushort firearmSerial, 
        [CommandParameter("Attachments", "List of attachments to enable.")] List<AttachmentName> attachments)
    {
        if (!InventoryExtensions.ServerTryGetItemWithSerial(firearmSerial, out var item)
            || item is not Firearm firearm || firearm == null)
        {
            Fail($"Could not find an active firearm with serial '{firearmSerial}'");
            return;
        }

        if (firearm.EnableAttachments(attachments))
        {
            Ok($"Successfully modified attachments of firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
            return;
        }
        
        Fail($"No modifications were made to firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})'");
    }
    
    /// <summary>
    /// Lists all attachments on a specific firearm.
    /// </summary>
    [CommandOverload("serial", "Lists all attachments on a specific firearm.", null)]
    public void Serial(
        [CommandParameter("Serial", "The serial number of the firearm item.")] ushort firearmSerial)
    {
        if (!InventoryExtensions.ServerTryGetItemWithSerial(firearmSerial, out var item)
            || item is not Firearm firearm || firearm == null)
        {
            Fail($"Could not find an active firearm with serial '{firearmSerial}'");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine(
                $"Showing attachments on firearm '{firearm.ItemSerial} ({firearm.ItemTypeId})', owned by {(ExPlayer.TryGet(firearm.Owner, out var owner) ? owner.ToCommandString() : "(null)")}");

            x.AppendLine($"Code: {firearm.GetCurrentAttachmentsCode()}");
            
            for (var i = 0; i < firearm.Attachments.Length; i++)
            {
                var attachment = firearm.Attachments[i];
                
                if (!attachment.IsEnabled)
                    continue;

                attachment.GetNameAndDescription(out var name, out var description);

                x.AppendLine();
                x.AppendLine($"- {attachment.Name}");
                x.AppendLine($"  > Name: {name}");
                x.AppendLine($"  > Slot: {attachment.Slot}");
                x.AppendLine($"  > Description: {description}");
                x.AppendLine($"  > Downsides: {attachment.DescriptiveCons}");
                x.AppendLine($"  > Advantages: {attachment.DescriptivePros}");
                x.AppendLine($"  > Weight: {attachment.Weight} kg");
                x.AppendLine($"  > Length: {attachment.Length} m");

                for (var y = 0; y < attachment._parameterStates.Length; y++)
                {
                    var state = attachment._parameterStates[y];
                    var value = attachment._parameterValues[y];
                    
                    if (state is AttachmentParamState.Disabled)
                        continue;

                    x.AppendLine($"    >- Parameter {y}: {state} ({value})");
                }
            }
        });
    }
    
    /// <summary>
    /// Lists all available attachments on a firearm.
    /// </summary>
    [CommandOverload("item", "Shows all attachments on an item.", null)]
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
            x.AppendLine($"Firearm '{firearm!.ItemTypeId}' ({firearm.Attachments?.Length ?? -1} attachments):");

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
                }
            }
        });
    }
}