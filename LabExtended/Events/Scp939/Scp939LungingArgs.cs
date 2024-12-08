using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp939;

namespace LabExtended.Events.Scp939
{
    public class Scp939LungingArgs : BoolCancellableEvent
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; set; }

        public Scp939Role Role { get; }
        public Scp939LungeAbility Ability { get; }

        public float LungeTargetDamage { get; set; } = 120f;
        public float LungeSecondaryDamage { get; set; } = 30f;

        internal Scp939LungingArgs(ExPlayer scp, ExPlayer target, Scp939Role role, Scp939LungeAbility ability)
            => (Scp, Target, Role, Ability) = (scp, target, role, ability);
    }
}