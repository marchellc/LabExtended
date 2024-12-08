namespace LabExtended.Core.Ticking.Interfaces
{
    public interface ITickStateInvoker<T> : ITickInvoker
    {
        ITickState<T> State { get; }
    }
}