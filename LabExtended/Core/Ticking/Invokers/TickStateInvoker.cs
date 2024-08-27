using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Core.Ticking.Invokers
{
    public class TickStateInvoker<T> : ITickStateInvoker<T>
    {
        public ITickState<T> State { get; }

        public volatile Action<T> Target;

        internal TickStateInvoker(ITickState<T> state, Action<T> target)
        {
            State = state;
            Target = target;
        }

        public void Invoke()
            => Target(State.GetState());

        public override string ToString()
            => $"{Target.Method.GetMemberName()} (State: {State})";
    }
}