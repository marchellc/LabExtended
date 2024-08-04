using InventorySystem.Items.Firearms;

using LabExtended.API;
using LabExtended.Core.Events;

using UnityEngine;

namespace LabExtended.Events.Player
{
    public class PlayerPerformingShotArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }
        public ExPlayer Target { get; }

        public Firearm Firearm { get; }

        public Ray Ray { get; }
        public RaycastHit Hit { get; }

        public IDestructible Destructible { get; }

        public float Damage { get; set; }

        public bool PlaceBulletDecal { get; set; } = true;
        public bool PlaceBloodDecal { get; set; } = true;

        public bool ShowIndicator { get; set; } = true;

        public bool SpawnExplosion { get; set; } = true;

        internal PlayerPerformingShotArgs(ExPlayer player, ExPlayer target, Firearm firearm, Ray ray, RaycastHit hit, IDestructible destructible, float damage)
            => (Player, Target, Firearm, Ray, Hit, Destructible, Damage) = (player, target, firearm, ray, hit, destructible, damage);
    }
}