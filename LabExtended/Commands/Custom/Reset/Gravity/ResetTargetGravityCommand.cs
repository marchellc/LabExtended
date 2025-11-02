using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetTargetCommand
{
    /// <summary>
    /// Resets the gravity setting for the specified player to the default value.
    /// </summary>
    /// <param name="target">The player whose gravity will be reset. If null, the command sender's gravity is reset.</param>
    [CommandOverload("gravity", "Resets the gravity of a specific player.", null)]
    public void GravityTarget(ExPlayer? target = null)
    {
        var player = target ?? Sender;

        player.Position.Gravity = PositionContainer.DefaultGravity;
        
        Ok($"Reset gravity of \"{player.Nickname}\" ({player.UserId})");
    }
}