using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetCommand
{
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