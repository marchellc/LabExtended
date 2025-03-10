using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Scp049
{
    public class Scp049CancellingResurrectionArgs : BoolCancellableEvent
    {
        public ExPlayer Scp { get; }
        public ExPlayer Target { get; }

        public byte ErrorCode { get; set; }

        internal Scp049CancellingResurrectionArgs(ExPlayer? scp, ExPlayer? target, byte code) => (Scp, Target, ErrorCode) = (scp, target, code);
    }
}