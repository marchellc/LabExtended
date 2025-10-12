using LabExtended.API;

using PlayerRoles;

namespace LabExtended.Events.Round
{
    /// <summary>
    /// Gets called before a player's role is set by late join.
    /// </summary>
    public class LateJoinSettingRoleEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// Gets the player who joined late.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// Gets or sets the role the player will be spawned as.
        /// </summary>
        public RoleTypeId Role { get; set; }

        /// <summary>
        /// Gets or sets the role's spawn flags.
        /// </summary>
        public RoleSpawnFlags Flags { get; set; }

        /// <summary>
        /// Initializes a new instance of the LateJoinSettingRoleEventArgs class with the specified player, role, and
        /// spawn flags.
        /// </summary>
        /// <param name="player">The player for whom the role is being set during a late join event. Cannot be null.</param>
        /// <param name="role">The role to assign to the player.</param>
        /// <param name="flags">The flags that control how the role is spawned for the player.</param>
        public LateJoinSettingRoleEventArgs(ExPlayer player, RoleTypeId role, RoleSpawnFlags flags)
        {
            Player = player;
            Role = role;
            Flags = flags;
        }
    }
}