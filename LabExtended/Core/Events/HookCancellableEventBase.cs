using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Events
{
    /// <summary>
    /// Represents a cancellable event.
    /// </summary>
    /// <typeparam name="T">The type of the event's cancellation.</typeparam>
    public class HookCancellableEventBase<T> : ICancellableEvent<T>
    {
        /// <summary>
        /// Gets the value that allows the event to continue.
        /// </summary>
        public virtual T AllowedValue { get; }

        /// <summary>
        /// Gets the value that prevents the event from continuing.
        /// </summary>
        public virtual T DeniedValue { get; }

        /// <summary>
        /// Gets or sets the event's cancellation status.
        /// </summary>
        public T Cancellation { get; set; }

        /// <summary>
        /// Allows the event to continue.
        /// </summary>
        public void Allow()
            => Cancellation = AllowedValue;

        /// <summary>
        /// Prevents the event from continuing.
        /// </summary>
        public void Cancel()
            => Cancellation = DeniedValue;

        internal virtual bool IsAllowed(T value)
            => false;
    }
}