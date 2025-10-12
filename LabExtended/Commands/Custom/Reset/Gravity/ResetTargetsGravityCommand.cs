using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetTargetsCommand
{
    [CommandOverload("gravity", "Resets the gravity of a list of players.", null)]
    public void GravityTarget(List<ExPlayer> targets)
    {
        targets.ForEach(p => p.Position.Gravity = PositionContainer.DefaultGravity);
        Ok($"Reset gravity of {targets.Count} player(s).");
    }
}