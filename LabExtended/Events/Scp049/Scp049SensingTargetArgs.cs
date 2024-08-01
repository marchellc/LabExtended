using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp049;

namespace LabExtended.Events.Scp049
{
    public class Scp049SensingTargetArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; set; }

        public Scp049Role Role { get; }
        public Scp049SenseAbility Ability { get; }

        public double Cooldown { get; set; } = 2.5;
        public double Duration { get; set; } = 20;

        internal Scp049SensingTargetArgs(ExPlayer scp, ExPlayer target, Scp049Role role, Scp049SenseAbility ability)
            => (Scp, Target, Role, Ability) = (scp, target, role, ability);
    }
}