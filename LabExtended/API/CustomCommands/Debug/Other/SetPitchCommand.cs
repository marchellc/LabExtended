using LabExtended.API.CustomVoice.Pitching;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Debug.Other;

public class SetPitchCommand : CustomCommand
{
    public override string Command { get; } = "setpitch";
    public override string Description { get; } = "Sets the global voice pitch.";

    public override ArgumentDefinition[] BuildArgs() => GetArg<float>("Pitch", "The pitch factor to set.");

    public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
    {
        base.OnCommand(sender, ctx, args);
        
        var pitch = args.Get<float>("Pitch");
        
        VoicePitch.GlobalPitch = pitch;
        
        ctx.RespondOk($"Global pitch changed to {VoicePitch.GlobalPitch}");
    }
}