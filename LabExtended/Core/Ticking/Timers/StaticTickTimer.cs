using LabExtended.Core.Ticking.Interfaces;

using System.Diagnostics;

namespace LabExtended.Core.Ticking.Timers
{
    public class StaticTickTimer : ITickTimer
    {
        private volatile Stopwatch _stopwatch;

        public readonly long Ticks = 0;

        public StaticTickTimer(long ticks)
        {
            Ticks = ticks;

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public StaticTickTimer(TimeSpan span) : this(span.Ticks) { }
        public StaticTickTimer(int milliseconds) : this(milliseconds * TimeSpan.TicksPerMillisecond) { }

        public bool CanContinue()
            => _stopwatch != null && (!_stopwatch.IsRunning || _stopwatch.ElapsedTicks > Ticks);

        public void OnExecuted()
            => _stopwatch.Restart();
        
        public void Dispose()
        {
            _stopwatch.Reset();
            _stopwatch = null;
        }

        public override string ToString()
            => $"StaticTimer Ticks={Ticks} ({Ticks / TimeSpan.TicksPerMillisecond} ms) Null={_stopwatch is null} Running={_stopwatch?.IsRunning ?? false} Elapsed={_stopwatch?.ElapsedTicks ?? -1}";
    }
}