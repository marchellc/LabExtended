using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetsCommand
{
    /// <summary>
    /// Sets the gravity vector for a list of targeted players.
    /// </summary>
    /// <param name="gravity">The new gravity vector to apply to each targeted player.</param>
    /// <param name="targets">The list of players whose gravity will be set. Cannot be null.</param>
    [CommandOverload("gravity", "Sets the gravity of a list of players.", null)]
    public void GravityTarget(
        [CommandParameter("Value", "The new gravity vector.")] Vector3 gravity,
        [CommandParameter("Targets", "List of targeted players.")] List<ExPlayer> targets)
    {
        targets.ForEach(p => p.Position.Gravity = gravity);
        Ok($"Set gravity of {targets.Count} player(s) to {gravity.ToPreciseString()}");
    }
}