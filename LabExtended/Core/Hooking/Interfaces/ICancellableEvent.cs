namespace LabExtended.Core.Hooking.Interfaces
{
    public interface ICancellableEvent<T> : IHookEvent
    {
        T IsAllowed { get; set; }
    }
}