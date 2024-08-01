using LabExtended.API;

namespace LabExtended.Core.Performance
{
    public class PerformanceReport
    {
        public PerformanceStat<object> PingWatcher { get; } = new PerformanceStat<object>();

        public PerformanceStat<object> TpsWatcher { get; } = new PerformanceStat<object>();
        public PerformanceStat<object> FrameTimeWatcher { get; } = new PerformanceStat<object>();

        public PerformanceStat<object> MemoryWatcher { get; } = new PerformanceStat<object>();

        public PerformanceStat<object> LoopWatcher { get; } = new PerformanceStat<object>();
        public PerformanceStat<object> ThreadWatcher { get; } = new PerformanceStat<object>();
        public PerformanceStat<object> ScriptWatcher { get; } = new PerformanceStat<object>();

        public void LogToConsole()
        {
            if (TpsWatcher.MinOverall < 0)
                return;

            ExLoader.Info("Performance API",
                $"Performance report for round number &3{ExRound.RoundNumber}&r (started at &6{ExRound.StartedAt}&r)\n" +
                $"&1Ping&r\n" +
                $" - &2Min Value Round&r: &6{PingWatcher.MinThisRound} ms&r (&3{TpsWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{PingWatcher.MinOverall} ms&r (&3{TpsWatcher.MinOverallPlayerCount} player(s)&r | &3{TpsWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{PingWatcher.MaxThisRound} ms&r (&3{TpsWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{PingWatcher.MaxOverall} ms&r (&3{TpsWatcher.MaxOverallPlayerCount} player(s)&r | &3{TpsWatcher.MaxOverallTime}&r)\n" +
                $"&1TPS&r\n" +
                $" - &2Min Value Round&r: &6{TpsWatcher.MinThisRound}&r (&3{TpsWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{TpsWatcher.MinOverall}&r (&3{TpsWatcher.MinOverallPlayerCount} player(s)&r | &3{TpsWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{TpsWatcher.MaxThisRound}&r (&3{TpsWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{TpsWatcher.MaxOverall}&r (&3{TpsWatcher.MaxOverallPlayerCount} player(s)&r | &3{TpsWatcher.MaxOverallTime}&r)\n" +
                $"&1Frame Time&r\n" +
                $" - &2Min Value Round&r: &6{FrameTimeWatcher.MinThisRound} ms&r (&3{FrameTimeWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{FrameTimeWatcher.MinOverall} ms&r (&3{FrameTimeWatcher.MinOverallPlayerCount} player(s)&r | &3{FrameTimeWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{FrameTimeWatcher.MaxThisRound} ms&r (&3{FrameTimeWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{FrameTimeWatcher.MaxOverall} ms&r (&3{FrameTimeWatcher.MaxOverallPlayerCount} player(s)&r | &3{FrameTimeWatcher.MaxOverallTime}&r)\n" +
                $"&1Memory Usage&r\n" +
                $" - &2Min Value Round&r: &6{Mirror.Utils.PrettyBytes(MemoryWatcher.MinThisRound)}&r (&3{MemoryWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{Mirror.Utils.PrettyBytes(MemoryWatcher.MinOverall)}&r (&3{MemoryWatcher.MinOverallPlayerCount} player(s)&r | &3{MemoryWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{Mirror.Utils.PrettyBytes(MemoryWatcher.MaxThisRound)}&r (&3{MemoryWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{Mirror.Utils.PrettyBytes(MemoryWatcher.MaxOverall)}&r (&3{MemoryWatcher.MaxOverallPlayerCount} player(s)&r | &3{MemoryWatcher.MaxOverallTime}&r)\n" +
                $"&1Loop Time&r\n" +
                $" - &2Min Value Round&r: &6{LoopWatcher.MinThisRound / 1000000} ms&r (&3{LoopWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{LoopWatcher.MinOverall / 1000000} ms&r (&3{LoopWatcher.MinOverallPlayerCount} player(s)&r | &3{LoopWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{LoopWatcher.MaxThisRound / 1000000} ms&r (&3{LoopWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{LoopWatcher.MaxOverall / 1000000} ms&r (&3{LoopWatcher.MaxOverallPlayerCount} player(s)&r | &3{LoopWatcher.MaxOverallTime}&r)\n" +
                $"&1Thread Time&r\n" +
                $" - &2Min Value Round&r: &6{ThreadWatcher.MinThisRound / 1000000} ms&r (&3{ThreadWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{ThreadWatcher.MinOverall / 1000000} ms&r (&3{ThreadWatcher.MinOverallPlayerCount} player(s)&r | &3{ThreadWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{ThreadWatcher.MaxThisRound / 1000000} ms&r (&3{ThreadWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{ThreadWatcher.MaxOverall / 1000000} ms&r (&3{ThreadWatcher.MaxOverallPlayerCount} player(s)&r | &3{ThreadWatcher.MaxOverallTime}&r)\n" +
                $"&1Script Time&r\n" +
                $" - &2Min Value Round&r: &6{ScriptWatcher.MinThisRound / 1000000} ms&r (&3{ScriptWatcher.MinRoundPlayerCount} player(s)&r)\n" +
                $" - &2Min Value Overall&r: &6{ScriptWatcher.MinOverall / 1000000} ms&r (&3{ScriptWatcher.MinOverallPlayerCount} player(s)&r | &3{ScriptWatcher.MinOverallTime}&r)\n" +
                $" - &1Max Value Round&r: &6{ScriptWatcher.MaxThisRound / 1000000} ms&r (&3{ScriptWatcher.MaxRoundPlayerCount} player(s)&r)\n" +
                $" - &1Max Value Overall&r: &6{ScriptWatcher.MaxOverall / 1000000} ms&r (&3{ScriptWatcher.MaxOverallPlayerCount} player(s)&r | &3{ScriptWatcher.MaxOverallTime}&r)");
        }
    }
}