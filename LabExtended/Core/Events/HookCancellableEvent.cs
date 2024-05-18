using LabExtended.Core.Hooking;

namespace LabExtended.Core.Events
{
    public class HookCancellableEvent<T> : HookEvent, ICancellableEvent<T>
    {
        private T _value;

        public HookInfo CancelledBy { get; private set; }
        public HookInfo AllowedBy { get; private set; }

        public virtual T DefaultValue { get; }
        public virtual T AllowedValue { get; }
        public virtual T CancelledValue { get; }

        public T IsCancelled
        {
            get => _value;
            set
            {
                _value = value;

                if (IsCancellation(value))
                {
                    AllowedBy = null;
                    CancelledBy = CurrentHook;
                }
                else
                {
                    CancelledBy = null;
                    AllowedBy = CurrentHook;
                }
            }
        }

        public void Allow(T value = default)
        {
            if (value is null)
                value = AllowedValue;

            IsCancelled = value;
        }

        public virtual void Cancel(T value = default)
        {
            if (value is null)
                value = CancelledValue;

            IsCancelled = value;
        }

        internal virtual bool IsCancellation(T value)
            => false;
    }
}