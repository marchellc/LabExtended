using LabExtended.API;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Core.Performance
{
    public class PerformanceReport
    {
        public PerformanceStat PingWatcher { get; } = new PerformanceStat("Average Ping", "average_ping", true, value => $"{value} ms");

        public PerformanceStat TpsWatcher { get; } = new PerformanceStat("TPS", "tps", false, value => $"{value} TPS");
        public PerformanceStat FrameTimeWatcher { get; } = new PerformanceStat("Frame Time", "frame_time", true, value => $"{value} ms");

        public PerformanceStat MemoryWatcher { get; } = new PerformanceStat("Used Memory", "used_memory", true, Mirror.Utils.PrettyBytes);

        public PerformanceStat LoopWatcher { get; } = new PerformanceStat("Loop Time", "loop_time", true, value => $"{value / 1000000} ms");
        public PerformanceStat ThreadWatcher { get; } = new PerformanceStat("Thread Time", "thread_time", true, value => $"{value / 1000000} ms");

        public void LogToConsole()
        {
            var builder = StringBuilderPool.Shared.Rent();
            var props = typeof(PerformanceReport).FindPropertiesOfType(typeof(PerformanceStat));

            builder.AppendLine($"Performance report for Round &4{ExRound.RoundNumber}&r (started at &6{ExRound.StartedAt}&r)");

            foreach (var prop in props)
            {
                var stat = prop.GetValue(this) as PerformanceStat;

                if (stat is null)
                    continue;

                if (stat.RoundValue.MinValue < 0)
                    continue;

                builder.AppendLine($"&4{stat.Name}&r");

                builder.AppendLine($" - &2Round Min&r: &6{stat.RoundValue.MinValueStr}&r (&3{stat.RoundValue.PlayerCountMin} player(s)&r | &3{stat.RoundValue.TimeMin}&r))");
                builder.AppendLine($" - &2Overall Min&r: &6{stat.OverallValue.MinValueStr}&r (&3{stat.OverallValue.PlayerCountMin} player(s)&r | &3{stat.OverallValue.TimeMin}&r))");

                builder.AppendLine($" - &1Round Max&r: &6{stat.RoundValue.MaxValueStr}&r (&3{stat.RoundValue.PlayerCountMax} player(s)&r | &3{stat.RoundValue.TimeMax}&r))");
                builder.AppendLine($" - &1Overall Max&r: &6{stat.OverallValue.MaxValueStr}&r (&3{stat.OverallValue.PlayerCountMax} player(s)&r | &3{stat.OverallValue.TimeMax}&r))");
            }

            ApiLoader.Info("Performance Log", StringBuilderPool.Shared.ToStringReturn(builder));
        }
    }
}