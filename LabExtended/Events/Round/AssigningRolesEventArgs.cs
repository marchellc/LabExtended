using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Round
{
    /// <summary>
    /// Gets called when the server starts assigning roles when the round starts.
    /// </summary>
    public class AssigningRolesEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// A dictionary of players and their decided roles.
        /// </summary>
        public Dictionary<ExPlayer?, RoleTypeId> Roles { get; }

        /// <summary>
        /// Creates a new <see cref="AssigningRolesEventArgs"/> instance.
        /// </summary>
        /// <param name="roles"></param>
        public AssigningRolesEventArgs(Dictionary<ExPlayer?, RoleTypeId> roles)
            => Roles = roles;
    }
}