using LabExtended.Core;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;

using MEC;

namespace LabExtended.Events
{
    /// <summary>
    /// A class that holds a simple frame update delegate.
    /// </summary>
    public static class UpdateEvent
    {
        private static readonly ProfilerMarker _marker = new ProfilerMarker("Update Event");
        private static CoroutineHandle _coroutine;

        /// <summary>
        /// Gets called every frame.
        /// </summary>
        public static event Action OnUpdate;

        internal static void Initialize()
            => _coroutine = Timing.RunCoroutine(Runner());

        internal static void KillEvent()
            => Timing.KillCoroutines(_coroutine);

        private static IEnumerator<float> Runner()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                _marker.MarkStart();

                try
                {
                    OnUpdate?.Invoke();
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Update Event", $"Failed to execute frame update due to an exception:\n{ex.ToColoredString()}");
                }

                _marker.MarkEnd();
            }
        }
    }
}