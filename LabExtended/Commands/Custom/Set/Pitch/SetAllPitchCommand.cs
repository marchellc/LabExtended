using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetAllCommand
{
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value)
    {
        ExPlayer.Players.ForEach(p => p.VoicePitch = value);
        Ok($"Set voice pitch of {ExPlayer.Count} player(s) to \"{value}\"");
    }
}