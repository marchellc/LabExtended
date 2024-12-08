using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player starts changing their role.
    /// </summary>
    public class PlayerSpawningArgs : BoolCancellableEvent
    {
        /// <summary>
        /// Gets the player who's changing their role.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets the player's current role.
        /// </summary>
        public PlayerRoleBase CurrentRole { get; }

        /// <summary>
        /// Gets or sets the new role's type.
        /// </summary>
        public RoleTypeId NewRole { get; set; }

        /// <summary>
        /// Gets or sets the role change reason.
        /// </summary>
        public RoleChangeReason ChangeReason { get; set; }

        /// <summary>
        /// Gets or sets the role spawn flags.
        /// </summary>
        public RoleSpawnFlags SpawnFlags { get; set; }

        internal PlayerSpawningArgs(ExPlayer player, PlayerRoleBase currentRole, RoleTypeId newRole, RoleChangeReason changeReason, RoleSpawnFlags spawnFlags)
        {
            Player = player;
            CurrentRole = currentRole;
            NewRole = newRole;
            ChangeReason = changeReason;
            SpawnFlags = spawnFlags;
        }
    }
}