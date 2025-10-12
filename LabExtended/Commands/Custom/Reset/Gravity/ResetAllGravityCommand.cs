using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.Commands.Attributes;

using UnityEngine;

namespace LabExtended.Commands.Custom.Reset;

public partial class ResetAllCommand
{
    [CommandOverload("gravity", "Resets the gravity of all players.", null)]
    public void GravityTarget()
    {
        PositionContainer.ResetGravity();
        Ok($"Reset gravity of {ExPlayer.AllCount} player(s).");
    }
}