using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.States
{
    public class TickStaticState<T> : ITickState<T>
    {
        private readonly T _state;

        public TickStaticState(T state)
            => _state = state;

        public T GetState()
            => _state;
    }
}