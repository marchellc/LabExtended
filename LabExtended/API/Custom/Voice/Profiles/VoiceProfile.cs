using PlayerRoles;

using VoiceChat.Networking;

namespace LabExtended.API.Custom.Voice.Profiles;

/// <summary>
/// Represents a voice profile.
/// </summary>
public abstract class VoiceProfile
{
    /// <summary>
    /// Whether or not the profile is active.
    /// </summary>
    public bool IsActive { get; internal set; }

    /// <summary>
    /// Gets the player who owns this profile.
    /// </summary>
    public ExPlayer Player { get; internal set; }

    /// <summary>
    /// Initializes the profile.
    /// </summary>
    public virtual void Start() { }

    /// <summary>
    /// Disposes the profile.
    /// </summary>
    public virtual void Stop() { }

    /// <summary>
    /// Gets called when the profile is enabled.
    /// </summary>
    public virtual void Enable() { }

    /// <summary>
    /// Gets called when the profile is disabled.
    /// </summary>
    public virtual void Disable() { }

    /// <summary>
    /// Gets called when a voice message is received from the player.
    /// </summary>
    /// <param name="message">The received voice message.</param>
    /// <returns>The result which determines the next operation.</returns>
    public abstract VoiceProfileResult ReceiveFrom(ref VoiceMessage message);

    /// <summary>
    /// Gets called before a voice message is sent to another player.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="player">The player receiving the message.</param>
    /// <returns>The result which determines the next operation.</returns>
    public abstract VoiceProfileResult SendTo(ref VoiceMessage message, ExPlayer player);

    /// <summary>
    /// Whether or not to keep the profile enabled when the player's role changes.
    /// </summary>
    /// <param name="newRoleType">The new role of the player.</param>
    /// <returns>true if the profile should be kept active</returns>
    public virtual bool EnabledOnRoleChange(RoleTypeId newRoleType) => false;
}