using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetsCommand
{
    [CommandOverload("gravity", "Sets the gravity of a list of players.", null)]
    public void GravityTarget(
        [CommandParameter("Value", "The new gravity vector.")] Vector3 gravity,
        [CommandParameter("Targets", "List of targeted players.")] List<ExPlayer> targets)
    {
        targets.ForEach(p => p.Position.Gravity = gravity);
        Ok($"Set gravity of {targets.Count} player(s) to {gravity.ToPreciseString()}");
    }
}