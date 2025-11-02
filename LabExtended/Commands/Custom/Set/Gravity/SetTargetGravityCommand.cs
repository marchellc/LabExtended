using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetCommand
{
    /// <summary>
    /// Sets the gravity vector for a specified player.
    /// </summary>
    /// <param name="gravity">The new gravity vector to apply to the player. Each component represents the force of gravity along the
    /// corresponding axis.</param>
    /// <param name="target">The player whose gravity will be set. If null, the gravity is set for the command sender.</param>
    [CommandOverload("gravity", "Sets the gravity of a specific player.", null)]
    public void GravityTarget(
        [CommandParameter("Value", "The new gravity vector.")]Vector3 gravity, 
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        var player = target ?? Sender;
        
        player.Position.Gravity = gravity;
        
        Ok($"Set gravity of \"{player.Nickname}\" ({player.UserId}) to {gravity.ToPreciseString()}");
    }
}