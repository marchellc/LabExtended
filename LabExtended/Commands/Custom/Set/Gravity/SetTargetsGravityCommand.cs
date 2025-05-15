using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetsCommand
{
    [CommandOverload("gravity", "Sets the gravity of a list of players.")]
    public void GravityTarget(Vector3 gravity, List<ExPlayer> targets)
    {
        targets.ForEach(p => p.Position.Gravity = gravity);
        Ok($"Set gravity of {targets.Count} player(s) to {gravity.ToPreciseString()}");
    }
}