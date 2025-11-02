using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomAmmo;

/// <summary>
/// Provides server-side commands for managing custom ammunition in player inventories, including retrieving, setting,
/// adding, removing, and clearing custom ammo amounts.
/// </summary>
/// <remarks>This command is intended for administrative use and allows direct manipulation of custom ammo values
/// for any player on the server. All subcommands default to affecting the command sender if no target player is
/// specified.</remarks>
[Command("customammo", "Custom Ammo management.", "cammo")]
public class CustomAmmoCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Displays the amount of custom ammo of a specified type in a player's inventory.
    /// </summary>
    /// <param name="ammoId">The identifier of the custom ammo type to query. Cannot be null or empty.</param>
    /// <param name="target">The player whose inventory will be checked. If null, the command sender is used.</param>
    [CommandOverload("get", "Gets the amount of custom ammo in a player's inventory.", null)]
    public void GetCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var amount = player.Ammo.GetCustomAmmo(ammoId);
        
        Ok($"Player \"{player.Nickname}\" ({player.ClearUserId}) has \"{amount}\" of ammo \"{ammoId}\".");
    }

    /// <summary>
    /// Sets the specified amount of custom ammunition for a player.
    /// </summary>
    /// <param name="ammoId">The identifier of the custom ammo type to set. Cannot be null or empty.</param>
    /// <param name="amount">The amount of ammo to set. Must be zero or greater.</param>
    /// <param name="target">The player whose ammo will be set. If null, the command sender is used.</param>
    [CommandOverload("set", "Sets a specific amount of custom ammo in a player's inventory.", null)]
    public void SetCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to set.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        
        player.Ammo.SetCustomAmmo(ammoId, amount);
        
        Ok($"Set ammo \"{ammoId}\" of player \"{player.Nickname}\" ({player.ClearUserId}) to \"{amount}\".");
    }
    
    /// <summary>
    /// Adds a specified amount of custom ammo to a player's inventory.
    /// </summary>
    /// <param name="ammoId">The identifier of the custom ammo type to add. Cannot be null or empty.</param>
    /// <param name="amount">The number of ammo units to add. Must be greater than zero.</param>
    /// <param name="target">The player who will receive the ammo. If null, the command sender is used.</param>
    [CommandOverload("add", "Adds a specific amount of custom ammo to a player's inventory.", null)]
    public void AddCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to add.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var current = player.Ammo.AddCustomAmmo(ammoId, amount);
        
        Ok($"Added \"{amount}\" of ammo \"{ammoId}\" to player \"{player.Nickname}\" ({player.ClearUserId}), new amount: \"{current}\".");
    }
    
    /// <summary>
    /// Removes a specified amount of custom ammunition from a player's inventory.
    /// </summary>
    /// <param name="ammoId">The identifier of the custom ammo type to remove. Cannot be null or empty.</param>
    /// <param name="amount">The number of ammo units to remove. Must be greater than zero.</param>
    /// <param name="target">The player from whose inventory the ammo will be removed. If null, the command sender is used.</param>
    [CommandOverload("Remove", "Removes a specific amount of custom ammo from a player's inventory.", null)]
    public void RemoveCommand(
        [CommandParameter("ID", "ID of the ammo.")] string ammoId,
        [CommandParameter("Amount", "Amount to remove.")] int amount,
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var current = player.Ammo.RemoveCustomAmmo(ammoId, amount);
        
        Ok($"Removed \"{amount}\" of ammo \"{ammoId}\" from player \"{player.Nickname}\" ({player.ClearUserId}), new amount: \"{current}\".");
    }
    
    /// <summary>
    /// Removes all custom ammo from the specified player's inventory.
    /// </summary>
    /// <param name="target">The player whose custom ammo inventory will be cleared. If null, the command sender's inventory is cleared.</param>
    [CommandOverload("clear", "Removes all custom ammo from a player's inventory.", null)]
    public void ClearCommand(
        [CommandParameter("Target", "The target player.")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        
        player.Ammo.ClearCustomAmmo();

        Ok($"Cleared Custom Ammo inventory of \"{player.Nickname}\" ({player.ClearUserId}).");
    }
}