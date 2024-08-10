using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Core.Ticking;

using Unity.Profiling;

using LabExtended.API.Collections.Locked;

using UnityEngine;

using System.Diagnostics;

namespace LabExtended.Core.Performance
{
    public static class PerformanceWatcher
    {
        public static readonly ProfilerRecorderOptions ProfilerOptions = ProfilerRecorderOptions.KeepAliveDuringDomainReload | ProfilerRecorderOptions.WrapAroundWhenCapacityReached | ProfilerRecorderOptions.SumAllSamplesInFrame | ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.CollectOnlyOnCurrentThread;

        private static ProfilerRecorder _playerLoop;
        private static ProfilerRecorder _mainThread;

        private static Process _curProcess;

        public const string PlayerLoopHandleName = "PlayerLoop";
        public const string MainThreadHandleName = "Main Thread";

        public static PerformanceReport CurReport { get; private set; } = new PerformanceReport();
        public static PerformanceReport PrevReport { get; private set; }

        public static LockedHashSet<PerformanceReport> AllReports { get; } = new LockedHashSet<PerformanceReport>();

        public static Process CurProcess => _curProcess;

        public static event Action<PerformanceReport> OnSubmitted;

        [OnLoad]
        public static void EnableWatcher()
        {
            try
            {
                _playerLoop = ProfilerRecorder.StartNew(new ProfilerCategory(PlayerLoopHandleName, ProfilerCategoryColor.Scripts), PlayerLoopHandleName, 1000, ProfilerOptions);
                _mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, MainThreadHandleName, 1000, ProfilerOptions);

                _curProcess = Process.GetCurrentProcess();

                TickManager.SubscribeTick(UpdateStats, TickTimer.GetStatic(1000f), "Performance Watcher");

                ApiLoader.Info("Performance API", "Performance Watcher enabled.");
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Performance API", $"Failed to enable the performance watcher!");
            }
        }

        public static void SubmitReport()
        {
            if (!ApiLoader.Config.ApiOptions.PerformanceOptions.EnablePerformanceWatcher)
                return;

            PrevReport = CurReport;
            PrevReport.LogToConsole();

            AllReports.Add(PrevReport);

            CurReport = new PerformanceReport();

            OnSubmitted?.Invoke(PrevReport);
        }

        private static void UpdateStats()
        {
            if (!ApiLoader.Config.ApiOptions.PerformanceOptions.EnablePerformanceWatcher)
                return;

            if (ExPlayer.Count > 0)
                CurReport.PingWatcher.Update((long)(ExPlayer._players.Average(x => x.TripTime)));

            CurReport.MemoryWatcher.Update(_curProcess.WorkingSet64);

            if (!ExServer.IsIdleModeActive)
            {
                CurReport.TpsWatcher.Update((long)ExServer.Tps);
                CurReport.FrameTimeWatcher.Update((long)(Time.deltaTime * 1000));
                CurReport.LoopWatcher.Update(_playerLoop.LastValue);
                CurReport.ThreadWatcher.Update(_mainThread.LastValue);
            }
        }
    }
}