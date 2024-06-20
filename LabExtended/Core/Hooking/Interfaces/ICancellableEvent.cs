namespace LabExtended.Core.Hooking.Interfaces
{
    public interface ICancellableEvent<T> : IHookEvent
    {
        T Cancellation { get; set; }

        void Cancel();
        void Allow();
    }
}