using LabExtended.API;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Debug.SetPitch;

/// <summary>
/// Sets a player's voice pitch.
/// </summary>
[Command("pitch", "Controls voice pitch.")]
public class SetPitchCommand : CommandBase, IServerSideCommand
{
    [CommandOverload("set", "Sets voice pitch of a specific player.")]
    public void SetOverload(
        [CommandParameter("Voice Pitch", "The new voice pitch value.")] float pitch, 
        [CommandParameter("Target", "The targeted player.")] ExPlayer? target = null)
    {
        target ??= Sender;
        target.VoicePitch = pitch;
        
        Ok($"Set voice pitch of player \"{target.Nickname} ({target.UserId})\" to \"{pitch}\".");
    }
    
    [CommandOverload("setall", "Applies the voice pitch to all players.")]
    public void SetAllOverload(
        [CommandParameter("Voice Pitch", "The new voice pitch value.")] float pitch)
    {
        ExPlayer.Players.ForEach(p => p.VoicePitch = pitch);
        
        Ok($"Set voice pitch of {ExPlayer.Count} player(s) to \"{pitch}\".");
    }

    [CommandOverload("reset", "Resets the voice pitch of a specific player.")]
    public void ResetOverload(
        [CommandParameter("Target", "The targeted player.")] ExPlayer? target = null)
    {
        target ??= Sender;
        target.VoicePitch = 1f;
        
        Ok($"Reset voice pitch of player \"{target.Nickname} ({target.UserId})\".");
    }

    [CommandOverload("resetall", "Resets the voice pitch of all players.")]
    public void ResetAllOverload()
    {
        ExPlayer.Players.ForEach(p => p.VoicePitch = 1f);
        
        Ok($"Reset voice pitch of {ExPlayer.Count} player(s).");
    }
}