using LabExtended.API;

using Mirror;

using PlayerRoles;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a new role starts initializing.
    /// </summary>
    public class PlayerChangingRoleArgs
    {
        /// <summary>
        /// Gets the player changing their role.
        /// </summary>
        public ExPlayer? Player { get; }

        /// <summary>
        /// Gets the player's previous role.
        /// </summary>
        public PlayerRoleBase PreviousRole { get; }

        /// <summary>
        /// Gets the player's new role.
        /// </summary>
        public PlayerRoleBase NewRole { get; }

        /// <summary>
        /// Gets or sets the role's change reason.
        /// </summary>
        public RoleChangeReason ChangeReason { get; set; }

        /// <summary>
        /// Gets or sets the role's spawn flags.
        /// </summary>
        public RoleSpawnFlags SpawnFlags { get; set; }

        /// <summary>
        /// Gets the role's spawn data.
        /// </summary>
        public NetworkReader SpawnData { get; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to give the player spawn protection.
        /// </summary>
        public bool GiveSpawnProtection { get; set; } = true;

        internal PlayerChangingRoleArgs(ExPlayer? player, PlayerRoleBase previousRole, PlayerRoleBase newRole, RoleChangeReason changeReason, RoleSpawnFlags spawnFlags, NetworkReader spawnData)
        {
            Player = player;
            PreviousRole = previousRole;
            NewRole = newRole;
            ChangeReason = changeReason;
            SpawnFlags = spawnFlags;
            SpawnData = spawnData;
        }
    }
}