using LabExtended.Core;
using LabExtended.Extensions;

using MEC;

namespace LabExtended.Utilities
{
    public static class UpdateEvent
    {
        public static event Action OnUpdate;

        public static TimeSpan LongestTick = TimeSpan.Zero;

        internal static void Initialize()
            => Timing.RunCoroutine(Runner());

        private static IEnumerator<float> Runner()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                var start = DateTime.Now;

                try
                {
                    OnUpdate?.Invoke();
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Update Event", $"Failed to execute frame update due to an exception:\n{ex.ToColoredString()}");
                }

                var duration = DateTime.Now - start;

                if (duration > LongestTick)
                {
                    if (LongestTick != TimeSpan.Zero)
                        ExLoader.Warn("Extended API", $"The update event has reached a new maximum duration (&3{LongestTick.TotalMilliseconds}&r &6->&r &3{duration.TotalMilliseconds} ms&r)");

                    LongestTick = duration;
                }
            }
        }
    }
}