using LabExtended.Core.Hooking.Interfaces;

namespace LabExtended.Core.Events
{
    /// <summary>
    /// Represents a cancellable event.
    /// </summary>
    /// <typeparam name="T">The type of the event's cancellation.</typeparam>
    public class CancellableEvent<T> : ICancellableEvent<T>
    {
        /// <summary>
        /// Gets or sets the event's cancellation status.
        /// </summary>
        public T IsAllowed { get; set; }
    }
}