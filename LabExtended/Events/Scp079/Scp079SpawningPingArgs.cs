using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Pinging;

using UnityEngine;

namespace LabExtended.Events.Scp079
{
    /// <summary>
    /// Gets called <b>BEFORE</b> SCP-079 spawns a ping indicator.
    /// </summary>
    public class Scp079SpawningPingArgs : HookBooleanCancellableEventBase
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

        internal Scp079SpawningPingArgs(ExPlayer player, Scp079Role scp079, Scp079PingAbility pingAbility, Scp079PingType pingType, Vector3 position, float cost)
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