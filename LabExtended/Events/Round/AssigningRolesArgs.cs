using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles;

namespace LabExtended.Events.Round
{
    /// <summary>
    /// Gets called when the server starts assigning roles when the round starts.
    /// </summary>
    public class AssigningRolesArgs : BoolCancellableEvent
    {
        /// <summary>
        /// A dictionary of player's and their decided role.
        /// </summary>
        public Dictionary<ExPlayer?, RoleTypeId> Roles { get; }

        internal AssigningRolesArgs(Dictionary<ExPlayer?, RoleTypeId> roles)
            => Roles = roles;
    }
}