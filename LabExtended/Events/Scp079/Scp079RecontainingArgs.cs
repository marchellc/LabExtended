using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Scp079
{
    public class Scp079RecontainingArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Activator { get; }

        public List<ExPlayer> Scps { get; }

        public bool PlayAnnouncement { get; set; } = true;

        public bool LockDoors { get; set; } = true;
        public bool FlickerLights { get; set; } = true;

        internal Scp079RecontainingArgs(ExPlayer activator, List<ExPlayer> scps) => (Activator, Scps) = (activator, scps);
    }
}