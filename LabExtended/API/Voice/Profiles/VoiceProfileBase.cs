using PlayerRoles;

using VoiceChat;

namespace LabExtended.API.Voice.Profiles
{
    /// <summary>
    /// Class used for managing custom voice profiles.
    /// </summary>
    public class VoiceProfileBase
    {
        /// <summary>
        /// Gets the owning player.
        /// </summary>
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether or not the profile is active.
        /// </summary>
        public bool IsActive { get; internal set; }

        /// <summary>
        /// Called when this profile gets added to a player.
        /// </summary>
        public virtual void OnStarted() { }

        /// <summary>
        /// Called when this profile gets removed from a player.
        /// </summary>
        public virtual void OnStopped() { }

        /// <summary>
        /// Called when the owning player sends a voice packet.
        /// </summary>
        /// <param name="dict">The list of voice channel destinations.</param>
        /// <param name="edit">The action used to edit the list.</param>
        public virtual void OnReceived(IReadOnlyDictionary<ExPlayer, VoiceChatChannel> dict, Action<ExPlayer, VoiceChatChannel> edit) { }

        /// <summary>
        /// Called when the owning player starts speaking.
        /// </summary>
        public virtual void OnStartedSpeaking() { }

        /// <summary>
        /// Called when the owning player stops speaking.
        /// </summary>
        /// <param name="startedAt">The time when the player started speaking.</param>
        /// <param name="speakingDuration">How long the player was speaking for.</param>
        /// <param name="capture">The packets captured while the player was speaking.</param>
        public virtual void OnStoppedSpeaking(DateTime startedAt, TimeSpan speakingDuration, byte[][] capture) { }

        /// <summary>
        /// Whether or not to keep this voice profiler when the player's role changes.
        /// </summary>
        /// <param name="newRole">The player's new role.</param>
        /// <returns><see langword="true"/> to keep the profile active, otherwise <see langword="false"/>.</returns>
        public virtual bool ShouldKeep(RoleTypeId newRole) => false;
    }
}