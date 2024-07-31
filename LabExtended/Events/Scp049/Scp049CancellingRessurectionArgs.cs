using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Scp049
{
    public class Scp049CancellingRessurectionArgs : HookBooleanCancellableEventBase
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; }

        public byte ErrorCode { get; set; }

        internal Scp049CancellingRessurectionArgs(ExPlayer scp, ExPlayer target, byte code) => (Scp, Target, ErrorCode) = (scp, target, code);
    }
}