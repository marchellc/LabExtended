using LabExtended.API;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Set;

public partial class SetAllCommand
{
    [CommandOverload("gravity", "Sets the gravity of all players.", null)]
    public void GravityTarget([CommandParameter("Value", "The new gravity vector.")] Vector3 gravity)
    {
        ExPlayer.AllPlayers.ForEach(p => p.Position.Gravity = gravity);
        Ok($"Set gravity of {ExPlayer.AllCount} player(s) to {gravity.ToPreciseString()}");
    }
}