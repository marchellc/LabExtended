using LabExtended.Core.Ticking.Internals;

namespace LabExtended.Core.Ticking.Interfaces
{
    public interface ITickDistributor : IDisposable
    {
        event Action OnTick;

        float TickRate { get; }
        float TickTime { get; }

        float MaxTickTime { get; }
        float MaxTickRate { get; }

        float MinTickTime { get; }
        float MinTickRate { get; }

        int HandleCount { get; }

        bool EnableTiming { get; set; }

        IEnumerable<string> Handles { get; }

        TickHandle CreateHandle(InternalTickHandle handle);

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