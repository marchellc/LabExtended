using LabExtended.API.Enums;
using LabExtended.API;

using PlayerRoles.PlayableScps.Scp079.Pinging;
using PlayerRoles.PlayableScps.Scp079;

using UnityEngine;

namespace LabExtended.Events.Scp079
{
    /// <summary>
    /// An event that gets called after SCP-079 spawns a ping indicator.
    /// </summary>
    public class Scp079SpawnedPingEventArgs : EventArgs
    {
        /// <summary>
        /// The player playing as SCP-079.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// The player's role.
        /// </summary>
        public Scp079Role Scp079 { get; }

        /// <summary>
        /// The player's ping ability.
        /// </summary>
        public Scp079PingAbility PingAbility { get; }

        /// <summary>
        /// Type of the spawned ping indicator.
        /// </summary>
        public Scp079PingType PingType { get; }

        /// <summary>
        /// Position of the spawned ping indicator.
        /// </summary>
        public Vector3 Position { get; }

        /// <summary>
        /// Creates a new <see cref="Scp079SpawnedPingEventArgs"/> instance.
        /// </summary>
        /// <param name="player">SCP-079 player.</param>
        /// <param name="scp079">SCP-079 role instance.</param>
        /// <param name="pingAbility">SCP-079 ping ability subroutine instance.</param>
        /// <param name="pingType">Spawned ping type.</param>
        /// <param name="position">Spawned ping position.</param>
        public Scp079SpawnedPingEventArgs(ExPlayer player, Scp079Role scp079, Scp079PingAbility pingAbility, 
            Scp079PingType pingType, Vector3 position)
        {
            Player = player;
            Scp079 = scp079;
            PingAbility = pingAbility;
            PingType = pingType;
            Position = position;
        }
    }
}