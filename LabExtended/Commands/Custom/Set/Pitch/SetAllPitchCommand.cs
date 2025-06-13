using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetAllCommand
{
    [CommandOverload("pitch", "Sets the voice pitch of all players.")]
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] int value)
    {
        ExPlayer.Players.ForEach(p => p.VoicePitch = value);
        Ok($"Set voice pitch of {ExPlayer.Count} player(s) to \"{value}\"");
    }
}