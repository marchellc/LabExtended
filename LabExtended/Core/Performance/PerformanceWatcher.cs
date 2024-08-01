using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Utilities.Values;

using LabExtended.Core.Ticking;

using MEC;

using Unity.Profiling;

using LabExtended.API.Collections.Locked;

namespace LabExtended.Core.Performance
{
    public static class PerformanceWatcher
    {
        public static readonly ProfilerRecorderOptions ProfilerOptions = ProfilerRecorderOptions.KeepAliveDuringDomainReload | ProfilerRecorderOptions.WrapAroundWhenCapacityReached | ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread;

        private static ProfilerRecorder _playerLoop;
        private static ProfilerRecorder _memUsed;
        private static ProfilerRecorder _mainThread;
        private static ProfilerRecorder _scripts;

        private static readonly LockedHashSet<string> _minLogged = new LockedHashSet<string>();
        private static readonly LockedHashSet<string> _maxLogged = new LockedHashSet<string>();

        public const string PlayerLoopHandleName = "PlayerLoop";
        public const string UsedMemoryHandleName = "Used Memory";
        public const string MainThreadHandleName = "Main Thread";
        public const string ScriptsHandleName = "<Uninitialized ProfilerMarker>";

        public static PerformanceReport CurReport { get; private set; } = new PerformanceReport();
        public static PerformanceReport PrevReport { get; private set; }

        public static LockedHashSet<PerformanceReport> AllReports { get; } = new LockedHashSet<PerformanceReport>();

        public static event Action<PerformanceReport> OnSubmitted;

        [OnLoad]
        public static void EnableWatcher()
        {
            Timing.CallDelayed(15f, () =>
            {
                try
                {
                    _playerLoop = ProfilerRecorder.StartNew(new ProfilerCategory(PlayerLoopHandleName, ProfilerCategoryColor.Scripts), PlayerLoopHandleName, 100, ProfilerOptions);
                    _memUsed = ProfilerRecorder.StartNew(ProfilerCategory.Memory, UsedMemoryHandleName, 1000, ProfilerOptions);
                    _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, MainThreadHandleName, 1000, ProfilerOptions);
                    _scripts = ProfilerRecorder.StartNew(ProfilerCategory.Scripts, ScriptsHandleName, 1000, ProfilerOptions);

                    TickManager.SubscribeTick(UpdateStats, TickTimer.GetStatic(1000f), "Performance Watcher");

                    ExLoader.Info("Performance API", "Performance Watcher enabled.");
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Performance API", $"Failed to enable the performance watcher!");
                }
            });
        }

        public static void SubmitReport()
        {
            _maxLogged.Clear();
            _minLogged.Clear();

            if (!ExLoader.Loader.Config.Api.EnablePerformanceWatcher)
                return;

            PrevReport = CurReport;
            PrevReport.LogToConsole();

            AllReports.Add(PrevReport);

            CurReport = new PerformanceReport();

            OnSubmitted?.Invoke(PrevReport);
        }

        private static void UpdateStats()
        {
            if (!ExLoader.Loader.Config.Api.EnablePerformanceWatcher || !ExRound.IsRunning)
                return;

            var curTps = (long)ExServer.Tps;
            var curTime = (long)ExServer.FrameTime;

            var curMem = _memUsed.LastValue;
            var curLoop = _playerLoop.LastValue;
            var curThread = _mainThread.LastValue;
            var curScripts = _scripts.LastValue;

            var avgPing = (long)(ExPlayer.Count > 0 ? ExPlayer.Players.Average(x => x.Ping) : 0);

            UpdateStat<object>(CurReport.PingWatcher, "ping", avgPing, null, null, isMin => isMin ? $"&Average Ping&r &2decreased&r to &6{avgPing} ms&r" : $"&Average Ping&r &1increased&r to &6{avgPing} ms&r");

            UpdateStat<object>(CurReport.TpsWatcher, "tps", (long)ExServer.Tps, null, null, isMin => isMin ? $"&3TPS&r &2decreased&r to &6{curTps}&r" : $"&3TPS&r &1increased&r to &6{curTps}&r");
            UpdateStat<object>(CurReport.FrameTimeWatcher, "frame_time", (long)ExServer.FrameTime, null, null, isMin => isMin ? $"&3Frame Time&r &2decreased&r to &6{curTime} ms&r" : $"&3Frame Time&r &1increased&r to &6{curTime} ms&r");

            UpdateStat<object>(CurReport.MemoryWatcher, "memory", curMem, null, null, isMin => isMin ? $"&3Used Memory&r &2decreased&r to &6{Mirror.Utils.PrettyBytes(curMem)}&r" : $"&3Used Memory&r &1increased&r to &6{Mirror.Utils.PrettyBytes(curMem)}&r");
            UpdateStat<object>(CurReport.LoopWatcher, "loop_time", curLoop, null, null, isMin => isMin ? $"&3Player Loop Time&r &2decreased&r to &6{curLoop / 1000000} ms&r" : $"&3Player Loop Time&r &1increased&r to &6{curLoop / 1000000} ms&r");
            UpdateStat<object>(CurReport.ThreadWatcher, "thread_time", curThread, null, null, isMin => isMin ? $"&3Main Thread Time&r &2decreased&r to &6{curThread / 1000000} ms&r" : $"&3Main Thread Time&r &1increased&r to &6{curThread / 1000000} ms&r");
            UpdateStat<object>(CurReport.ScriptWatcher, "script_time", curScripts, null, null, isMin => isMin ? $"&3Script Time&r &2decreased&r to &6{curScripts / 1000000} ms&r" : $"&3Script Time&r &1increased&r to &6{curScripts / 1000000} ms&r");
        }

        private static void UpdateStat<T>(PerformanceStat<T> stat, string name, long curValue, T minData = default, T maxData = default, Func<bool, string> formatText = null)
        {
            if (stat.MinThisRound < 0 || stat.MinThisRound > curValue)
            {
                stat.MinThisRound = curValue;
                stat.MinThisRoundData = OptionalValue<T>.FromValue(minData);
                stat.MinRoundPlayerCount = ExPlayer.Count;

                if (ExLoader.Loader.Config.Api.NotifyRoundOnMinChanged && formatText != null && !ExLoader.Loader.Config.Api.NoLogPerformance.Contains(name)
                    && (!ExLoader.Loader.Config.Api.LogOncePerformance.Contains(name) || !_minLogged.Contains(name)))
                {
                    var msg = formatText(true);

                    if (msg != null)
                        ExLoader.Info("Performance API", msg);
                }
            }

            if (stat.MaxThisRound < 0 || stat.MaxThisRound < curValue)
            {
                stat.MaxThisRound = curValue;
                stat.MaxThisRoundData = OptionalValue<T>.FromValue(maxData);
                stat.MaxRoundPlayerCount = ExPlayer.Count;

                if (ExLoader.Loader.Config.Api.NotifyRoundOnMaxChanged && formatText != null && !ExLoader.Loader.Config.Api.NoLogPerformance.Contains(name)
                    && (!ExLoader.Loader.Config.Api.LogOncePerformance.Contains(name) || !_maxLogged.Contains(name)))
                {
                    var msg = formatText(false);

                    if (msg != null)
                        ExLoader.Warn("Performance API", msg);
                }
            }

            if (stat.MinOverall < 0 || stat.MinOverall > curValue)
            {
                stat.MinOverall = curValue;
                stat.MinOverallTime = DateTime.Now;
                stat.MinOverallData = OptionalValue<T>.FromValue(minData);
                stat.MinOverallPlayerCount = ExPlayer.Count;

                if (ExLoader.Loader.Config.Api.NotifyOverallOnMinChanged && formatText != null && formatText != null && !ExLoader.Loader.Config.Api.NoLogPerformance.Contains(name)
                    && (!ExLoader.Loader.Config.Api.LogOncePerformance.Contains(name) || !_minLogged.Contains(name)))
                {
                    var msg = formatText(true);

                    if (msg != null)
                        ExLoader.Info("Performance API", msg);
                }
            }

            if (stat.MaxOverall < 0 || stat.MaxOverall < curValue)
            {
                stat.MaxOverall = curValue;
                stat.MaxOverallTime = DateTime.Now;
                stat.MaxOverallData = OptionalValue<T>.FromValue(maxData);
                stat.MaxOverallPlayerCount = ExPlayer.Count;

                if (ExLoader.Loader.Config.Api.NotifyOverallOnMaxChanged && formatText != null && formatText != null && !ExLoader.Loader.Config.Api.NoLogPerformance.Contains(name)
                    && (!ExLoader.Loader.Config.Api.LogOncePerformance.Contains(name) || !_maxLogged.Contains(name)))
                {
                    var msg = formatText(true);

                    if (msg != null)
                        ExLoader.Warn("Performance API", msg);
                }
            }
        }
    }
}