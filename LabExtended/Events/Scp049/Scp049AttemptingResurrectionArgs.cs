using LabExtended.API;
using LabExtended.Core.Events;

using PlayerRoles.PlayableScps.Scp049;

namespace LabExtended.Events.Scp049
{
    public class Scp049AttemptingResurrectionArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; }

        public Scp049ResurrectAbility.ResurrectError Error { get; set; }

        internal Scp049AttemptingResurrectionArgs(ExPlayer scp, ExPlayer target, Scp049ResurrectAbility.ResurrectError error) => (Scp, Target, Error) = (scp, target, error);
    }
}