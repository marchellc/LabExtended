using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Core.Ticking.Invokers
{
    public class TickInvoker : ITickInvoker
    {
        public volatile Action Target;

        public void Invoke()
            => Target();

        public override string ToString()
            => Target.Method.GetMemberName();
    }
}