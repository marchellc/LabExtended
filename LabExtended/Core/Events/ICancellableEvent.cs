using LabExtended.Core.Hooking;

namespace LabExtended.Core.Events
{
    public interface ICancellableEvent<T>
    {
        HookInfo CancelledBy { get; }
        HookInfo AllowedBy { get; }

        T DefaultValue { get; }

        T AllowedValue { get; }
        T CancelledValue { get; }

        T IsCancelled { get; set; }

        void Cancel(T value = default);
        void Allow(T value = default);
    }
}