using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Events
{
    public class HookCancellableEventBase<T> : ICancellableEvent<T>
    {
        public virtual T AllowedValue { get; }
        public virtual T DeniedValue { get; }

        public T Cancellation { get; set; }

        public void Allow()
            => Cancellation = AllowedValue;

        public void Cancel()
            => Cancellation = DeniedValue;

        internal virtual bool IsAllowed(T value)
            => false;
    }
}