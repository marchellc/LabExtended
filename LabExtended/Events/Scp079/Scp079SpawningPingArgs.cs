using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Pinging;

using UnityEngine;

namespace LabExtended.Events.Scp079
{
    public class Scp079SpawningPingArgs : HookBooleanCancellableEvent
    {
        public ExPlayer Player { get; }

        public Scp079Role Scp079 { get; }
        public Scp079PingAbility PingAbility { get; }

        public Scp079PingType PingType { get; set; }

        public Vector3 Position { get; set; }

        public float AuxCost { get; set; }

        public Scp079SpawningPingArgs(ExPlayer player, Scp079Role scp079, Scp079PingAbility pingAbility, Scp079PingType pingType, Vector3 position, float cost) : base(true)
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