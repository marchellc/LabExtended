using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetTargetsCommand
{
    /// <summary>
    /// Sets the voice pitch for the specified players to the given value.
    /// </summary>
    /// <param name="value">The new pitch value to assign to each player. A value of 1 represents the default pitch. Values greater than 1
    /// increase the pitch; values less than 1 decrease it.</param>
    /// <param name="targets">The list of players whose voice pitch will be set.</param>
    [CommandOverload("pitch", "Sets the voice pitch of a list of players.", null)]
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value,
        [CommandParameter("Targets", "The target players.")] List<ExPlayer> targets)
    {
        targets.ForEach(p => p.VoicePitch = value);
        Ok($"Set voice pitch of {targets.Count} player(s) to \"{value}\"");
    }
}