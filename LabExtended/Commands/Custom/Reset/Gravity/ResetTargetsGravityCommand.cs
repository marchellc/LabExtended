using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetTargetsCommand
{
    /// <summary>
    /// Resets the gravity setting for each player in the specified list to the default value.
    /// </summary>
    /// <param name="targets">The list of players whose gravity will be reset. Cannot be null.</param>
    [CommandOverload("gravity", "Resets the gravity of a list of players.", null)]
    public void GravityTarget(List<ExPlayer> targets)
    {
        targets.ForEach(p => p.Position.Gravity = PositionContainer.DefaultGravity);
        Ok($"Reset gravity of {targets.Count} player(s).");
    }
}