using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Set;

public partial class SetAllCommand
{
    /// <summary>
    /// Sets the voice pitch for all players to the specified value.
    /// </summary>
    /// <param name="value">The new pitch value to assign to all players. A value of 1 represents the default pitch. Values greater than 1
    /// increase the pitch; values less than 1 decrease it.</param>
    [CommandOverload("pitch", "Sets the voice pitch of all players.", null)]
    public void PitchTarget(
        [CommandParameter("Value", "The new pitch value (1 is default).")] float value)
    {
        ExPlayer.Players.ForEach(p => p.VoicePitch = value);
        Ok($"Set voice pitch of {ExPlayer.Count} player(s) to \"{value}\"");
    }
}