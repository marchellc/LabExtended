using LabExtended.API;

using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Events.Scp939
{
    /// <summary>
    /// Gets called before SCP-939 places a Mimicry.
    /// </summary>
    public class Scp939PlacingMimicryEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-939.
        /// </summary>
        public ExPlayer Scp { get; }

        /// <summary>
        /// SCP-939 role instance.
        /// </summary>
        public Scp939Role Role { get; }
        
        /// <summary>
        /// <see cref="MimicPointController"/> subroutine instance.
        /// </summary>
        public MimicPointController Controller { get; }

        /// <summary>
        /// Gets or sets the position of the Mimicry.
        /// </summary>
        public RelativePosition Position { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp939PlacingMimicryEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-939 player.</param>
        /// <param name="position">Mimicry position.</param>
        public Scp939PlacingMimicryEventArgs(ExPlayer? scp, RelativePosition position)
            => (Scp, Role, Controller, Position) = (scp, scp.Role.Scp939, scp.Subroutines.MimicPointController, position);
    }
}
