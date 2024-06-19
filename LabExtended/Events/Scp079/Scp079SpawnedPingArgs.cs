using LabExtended.API.Enums;
using LabExtended.API;

using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp079.Pinging;
using PlayerRoles.PlayableScps.Scp079;

using UnityEngine;

namespace LabExtended.Events.Scp079
{
    public class Scp079SpawnedPingArgs : HookEvent
    {
        public ExPlayer Player { get; }

        public Scp079Role Scp079 { get; }
        public Scp079PingAbility PingAbility { get; }

        public Scp079PingType PingType { get; }

        public Vector3 Position { get; }

        public Scp079SpawnedPingArgs(ExPlayer player, Scp079Role scp079, Scp079PingAbility pingAbility, Scp079PingType pingType, Vector3 position)
        {
            Player = player;
            Scp079 = scp079;
            PingAbility = pingAbility;
            PingType = pingType;
            Position = position;
        }
    }
}