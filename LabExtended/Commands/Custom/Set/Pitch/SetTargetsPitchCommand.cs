using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetsCommand
{
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> targets)
    {
        targets.ForEach(p => p.VoicePitch = value);
        Ok($"Set voice pitch of {targets.Count} player(s) to \"{value}\"");
    }
}