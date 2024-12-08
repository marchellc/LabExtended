using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Extensions;

using System.Diagnostics;

namespace LabExtended.Core.Ticking.Timers
{
    public class DynamicTickTimer : ITickTimer
    {
        private volatile Stopwatch _stopwatch;
        private volatile Func<long> _time;

        public long Ticks;

        public DynamicTickTimer(Func<long> time)
        {
            if (time is null)
                throw new ArgumentNullException(nameof(time));

            _time = time;

            Ticks = _time();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public bool CanContinue()
            => _stopwatch != null && (!_stopwatch.IsRunning || _stopwatch.ElapsedTicks > Ticks);

        public void OnExecuted()
        {
            _stopwatch.Restart();

            Ticks = _time();
        }

        public void Dispose()
        {
            _stopwatch.Reset();
            _stopwatch = null;

            _time = null;

            Ticks = 0;
        }

        public override string ToString()
            => $"DynamicTickTimer Timer={_time.Method.GetMemberName()} Ticks={Ticks} ({Ticks / TimeSpan.TicksPerMillisecond} ms) Null={_stopwatch is null} Running={_stopwatch?.IsRunning ?? false} Elapsed={_stopwatch?.ElapsedTicks ?? -1}";
    }
}