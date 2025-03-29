using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using UnityEngine;

namespace LabExtended.Commands.Custom.Debug.SetGravity;

/// <summary>
/// Sets a player's gravity.
/// </summary>
[Command("gravity", "Controls gravity.")]
public class SetGravityCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("set", "Sets gravity of a specific player.")]
    public void SetOverload(
        [CommandParameter("Gravity", "The new gravity value.")] Vector3 gravity, 
        [CommandParameter("Target", "The targeted player.")] ExPlayer? target = null)
    {
        target ??= Sender;
        target.Position.Gravity = gravity;
        
        Ok($"Set gravity of player \"{target.Nickname} ({target.UserId})\" to \"{gravity}\".");
    }
    
    [CommandOverload("setall", "Applies the gravity to all players.")]
    public void SetAllOverload(
        [CommandParameter("Gravity", "The new gravity value.")] Vector3 gravity)
    {
        PositionContainer.SetGravity(gravity);
        
        Ok($"Set gravity of {ExPlayer.Count} player(s) to \"{gravity}\".");
    }

    [CommandOverload("reset", "Resets the gravity of a specific player.")]
    public void ResetOverload(
        [CommandParameter("Target", "The targeted player.")] ExPlayer? target = null)
    {
        target ??= Sender;
        target.Position.Gravity = PositionContainer.DefaultGravity;
        
        Ok($"Reset gravity of player \"{target.Nickname} ({target.UserId})\".");
    }

    [CommandOverload("resetall", "Resets the gravity of all players.")]
    public void ResetAllOverload()
    {
        PositionContainer.ResetGravity();
        
        Ok($"Reset gravity of {ExPlayer.Count} player(s).");
    }
}