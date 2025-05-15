using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetCommand
{
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value,
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        var player = target ?? Sender;

        player.VoicePitch = value;
        
        Ok($"Set voice pitch of \"{player.Nickname}\" ({player.ClearUserId}) to \"{player.VoicePitch}\"");
    }
}