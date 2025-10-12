using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Round
{
    /// <summary>
    /// Gets called after the server starts assigns roles when the round starts.
    /// </summary>
    public class AssignedRolesEventArgs : EventArgs
    {
        /// <summary>
        /// A dictionary of players and their decided roles.
        /// </summary>
        public IReadOnlyDictionary<ExPlayer, RoleTypeId> Roles { get; }

        /// <summary>
        /// Creates a new <see cref="AssignedRolesEventArgs"/> instance.
        /// </summary>
        /// <param name="roles">The assigned roles.</param>
        public AssignedRolesEventArgs(Dictionary<ExPlayer, RoleTypeId> roles)
            => Roles = roles;
    }
}