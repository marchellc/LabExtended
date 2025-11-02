using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetAllCommand
{
    /// <summary>
    /// Sets the gravity vector for all players to the specified value.
    /// </summary>
    /// <param name="gravity">The new gravity vector to apply to all players. Each component represents the acceleration due to gravity along
    /// the corresponding axis.</param>
    [CommandOverload("gravity", "Sets the gravity of all players.", null)]
    public void GravityTarget([CommandParameter("Value", "The new gravity vector.")] Vector3 gravity)
    {
        ExPlayer.AllPlayers.ForEach(p => p.Position.Gravity = gravity);
        Ok($"Set gravity of {ExPlayer.AllCount} player(s) to {gravity.ToPreciseString()}");
    }
}