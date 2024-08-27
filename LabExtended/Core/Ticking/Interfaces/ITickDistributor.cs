namespace LabExtended.Core.Ticking.Interfaces
{
    public interface ITickDistributor : IDisposable
    {
        event Action OnTick;

        float TickRate { get; }

        int HandleCount { get; }

        TickHandle CreateHandle(object handle);

        void RemoveHandle(TickHandle handle);

        bool HasHandle(TickHandle handle);

        bool IsActive(TickHandle handle);
        bool IsPaused(TickHandle handle);

        void Pause(TickHandle handle);
        void Resume(TickHandle handle);

        void ClearHandles();
        void ClearEvent();
    }
}