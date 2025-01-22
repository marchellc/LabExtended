using LabExtended.API.Containers;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using PlayerRoles.FirstPersonControl;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Debug.Other;

public class SetGravityCommand : CustomCommand
{
    public override string Command { get; } = "setgravity";
    public override string Description { get; } = "Sets the gravity for all players.";

    public override ArgumentDefinition[] BuildArgs() => GetArg<Vector3>("Gravity", $"The gravity to set (default is {FpcGravityController.DefaultGravity.ToPreciseString()})");

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        PositionContainer.SetGravity(args.Get<Vector3>("Gravity"));
        
        ctx.RespondOk("Gravity changed.");
    }
}