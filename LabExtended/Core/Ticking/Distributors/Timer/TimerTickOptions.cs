using LabExtended.Core.Ticking.Interfaces;

namespace LabExtended.Core.Ticking.Distributors.Timer
{
    public class TimerTickOptions : ITickOptions
    {
        public volatile System.Timers.Timer Timer;

        public override string ToString()
            => Timer is null ? "Timer=null" : $"Timer=[Enabled={Timer.Enabled}, Interval={Timer.Interval}ms]";
    }
}