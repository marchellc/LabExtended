using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomAmmo;

[Command("customammo", "Custom Ammo management.", "cammo")]
public class CustomAmmoCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("get", "Gets the amount of custom ammo in a player's inventory.")]
    public void GetCommand(
        [CommandParameter("ID", "ID of the ammo.")] ushort ammoId,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var amount = player.Inventory.CustomAmmo.Get(ammoId);
        
        Ok($"Player \"{player.Nickname}\" ({player.ClearUserId}) has \"{amount}\" of ammo \"{ammoId}\".");
    }

    [CommandOverload("set", "Sets a specific amount of custom ammo in a player's inventory.")]
    public void SetCommand(
        [CommandParameter("ID", "ID of the ammo.")] ushort ammoId,
        [CommandParameter("Amount", "Amount to set.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        
        player.Inventory.CustomAmmo.Set(ammoId, amount);
        
        Ok($"Set ammo \"{ammoId}\" of player \"{player.Nickname}\" ({player.ClearUserId}) to \"{amount}\".");
    }
    
    [CommandOverload("add", "Adds a specific amount of custom ammo to a player's inventory.")]
    public void AddCommand(
        [CommandParameter("ID", "ID of the ammo.")] ushort ammoId,
        [CommandParameter("Amount", "Amount to add.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var current = player.Inventory.CustomAmmo.Add(ammoId, amount);
        
        Ok($"Added \"{amount}\" of ammo \"{ammoId}\" to player \"{player.Nickname}\" ({player.ClearUserId}), new amount: \"{current}\".");
    }
    
    [CommandOverload("Remove", "Removes a specific amount of custom ammo from a player's inventory.")]
    public void RemoveCommand(
        [CommandParameter("ID", "ID of the ammo.")] ushort ammoId,
        [CommandParameter("Amount", "Amount to remove.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var current = player.Inventory.CustomAmmo.Remove(ammoId, amount);
        
        Ok($"Removed \"{amount}\" of ammo \"{ammoId}\" from player \"{player.Nickname}\" ({player.ClearUserId}), new amount: \"{current}\".");
    }
    
    [CommandOverload("clear", "Removes all custom ammo from a player's inventory.")]
    public void ClearCommand(
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        
        player.Inventory.CustomAmmo.Clear();
        
        Ok($"Cleared Custom Ammo inventory of \"{player.Nickname}\" ({player.ClearUserId}).");
    }
}