using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetTargetCommand
{
    [CommandOverload("gravity", "Resets the gravity of a specific player.", null)]
    public void GravityTarget(ExPlayer? target = null)
    {
        var player = target ?? Sender;

        player.Position.Gravity = PositionContainer.DefaultGravity;
        
        Ok($"Reset gravity of \"{player.Nickname}\" ({player.UserId})");
    }
}