using LabExtended.API;

using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.PlayableScps.Scp939.Mimicry;

using RelativePositioning;

namespace LabExtended.Events.Scp939
{
    /// <summary>
    /// Gets called before SCP-939 removes a spawned Mimicry instance. 
    /// </summary>
    public class Scp939RemovingMimicryEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player playing as SCP-939.
        /// </summary>
        public ExPlayer Scp { get; }

        /// <summary>
        /// The SCP-939 role instance.
        /// </summary>
        public Scp939Role Role { get; }
        
        /// <summary>
        /// The <see cref="MimicPointController"/> subroutine instance.
        /// </summary>
        public MimicPointController Controller { get; }

        /// <summary>
        /// Gets or sets the position of the mimicry.
        /// </summary>
        public RelativePosition Position { get; set; }

        /// <summary>
        /// The reason for the removal.
        /// </summary>
        public MimicPointController.RpcStateMsg Reason { get; }

        /// <summary>
        /// Creates a new <see cref="Scp939RemovingMimicryEventArgs"/> instance.
        /// </summary>
        /// <param name="scp">SCP-939 player.</param>
        /// <param name="position">Mimicry position.</param>
        /// <param name="reason">Removal reason.</param>
        public Scp939RemovingMimicryEventArgs(ExPlayer scp, RelativePosition position, MimicPointController.RpcStateMsg reason)
            => (Scp, Role, Controller, Position, Reason) = (scp, scp.Role.Scp939, scp.Subroutines.MimicPointController, position, reason);
    }
}
