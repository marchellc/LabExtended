namespace LabExtended.Core.Hooking.Interfaces
{
    public interface ICancellableEvent<T>
    {
        T IsAllowed { get; set; }
    }
}