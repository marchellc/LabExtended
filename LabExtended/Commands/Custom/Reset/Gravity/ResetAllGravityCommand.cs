using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetAllCommand
{
    /// <summary>
    /// Resets the gravity settings for all players to their default values.
    /// </summary>
    /// <remarks>This method affects all currently active players. Use this command to restore normal gravity
    /// if it has been modified for any player.</remarks>
    [CommandOverload("gravity", "Resets the gravity of all players.", null)]
    public void GravityTarget()
    {
        PositionContainer.ResetGravity();
        Ok($"Reset gravity of {ExPlayer.AllCount} player(s).");
    }
}