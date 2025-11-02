using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetCommand
{
    /// <summary>
    /// Sets the voice pitch for the specified player.
    /// </summary>
    /// <param name="value">The new pitch value to assign. A value of 1 represents the default pitch. Values greater than 1 increase the
    /// pitch; values less than 1 decrease it.</param>
    /// <param name="target">The player whose voice pitch will be set. If null, the pitch is set for the command sender.</param>
    [CommandOverload("pitch", "Sets the voice pitch of a specific player.", null)]
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value,
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        var player = target ?? Sender;

        player.VoicePitch = value;
        
        Ok($"Set voice pitch of \"{player.Nickname}\" ({player.ClearUserId}) to \"{player.VoicePitch}\"");
    }
}