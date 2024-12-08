using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.States
{
    public class TickDynamicState<T> : ITickState<T>
    {
        private Func<T> _state;

        public TickDynamicState(Func<T> state)
            => _state = state;

        public T GetState()
            => _state();

        public override string ToString()
            => $"DynamicState ({typeof(T).FullName})";
    }
}