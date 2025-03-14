using LabExtended.API;
using LabExtended.API.Enums;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Pinging;

using UnityEngine;

namespace LabExtended.Events.Scp079
{
    /// <summary>
    /// Gets called <b>BEFORE</b> SCP-079 spawns a ping indicator.
    /// </summary>
    public class Scp079SpawningPingEventArgs : BooleanEventArgs
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
        /// Gets or sets the type of the ping indicator to spawn.
        /// </summary>
        public Scp079PingType PingType { get; set; }

        /// <summary>
        /// Gets or sets the position to spawn the ping indicator at.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the amount of AUX to substract.
        /// </summary>
        public float AuxCost { get; set; }

        /// <summary>
        /// Creates a new <see cref="Scp079SpawningPingEventArgs"/> instance.
        /// </summary>
        /// <param name="player">SCP-079 player.</param>
        /// <param name="scp079">SCP-079 role instance.</param>
        /// <param name="pingAbility">SCP-079 ping ability subroutine instance.</param>
        /// <param name="pingType">Spawned ping type.</param>
        /// <param name="position">Spawned ping position.</param>
        /// <param name="cost">Ability AUX cost.</param>
        public Scp079SpawningPingEventArgs(ExPlayer player, Scp079Role scp079, Scp079PingAbility pingAbility, 
            Scp079PingType pingType, Vector3 position, float cost)
        {
            Player = player;
            Scp079 = scp079;
            PingAbility = pingAbility;
            PingType = pingType;
            Position = position;
            AuxCost = cost;
        }
    }
}