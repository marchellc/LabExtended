using LabExtended.API;

namespace LabExtended.Events.Scp079
{
    /// <summary>
    /// Gets called before SCP-079 is contained.
    /// </summary>
    public class Scp079RecontainingEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player who pressed the recontainment button.
        /// </summary>
        public ExPlayer Activator { get; }

        /// <summary>
        /// List of other surviving SCPs.
        /// </summary>
        public List<ExPlayer> Scps { get; }

        /// <summary>
        /// Whether or not to play the termination announcement.
        /// </summary>
        public bool PlayAnnouncement { get; set; } = true;

        /// <summary>
        /// Whether or not to lock facility doors.
        /// </summary>
        public bool LockDoors { get; set; } = true;
        
        /// <summary>
        /// Whether or not to flicker facility lights.
        /// </summary>
        public bool FlickerLights { get; set; } = true;

        /// <summary>
        /// Creates a new <see cref="Scp079RecontainingEventArgs"/> instance.
        /// </summary>
        /// <param name="activator">The player who activated it.</param>
        /// <param name="scps">List of surviving SCPs.</param>
        public Scp079RecontainingEventArgs(ExPlayer activator, List<ExPlayer> scps) 
            => (Activator, Scps) = (activator, scps);
    }
}