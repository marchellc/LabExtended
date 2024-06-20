using LabExtended.API;
using LabExtended.API.Map;
using LabExtended.API.Npcs;
using LabExtended.API.Npcs.Navigation;
using LabExtended.API.Prefabs;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Round;

using LabExtended.Core;
using LabExtended.Core.Profiling;

using LabExtended.Patches.Functions;

using PluginAPI.Events;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A class used for round event delegates. These delegates get called before any other event handlers.
    /// </summary>
    public static class RoundEvents
    {
        /// <summary>
        /// Gets called when the round ends.
        /// </summary>
        public static event Action<RoundEndEvent> OnRoundEnded;

        /// <summary>
        /// Gets called when the round starts.
        /// </summary>
        public static event Action<RoundStartEvent> OnRoundStarted;

        /// <summary>
        /// Gets called when the round starts restarting.
        /// </summary>
        public static event Action<RoundRestartEvent> OnRoundRestarted;

        /// <summary>
        /// Gets called when the round starts waiting for players.
        /// </summary>
        public static event Action<WaitingForPlayersEvent> OnWaitingForPlayers;

        static RoundEvents()
        {
            OnWaitingForPlayers += InternalHandleWaiting;
            OnRoundRestarted += InternalHandleRestarting;
            OnRoundStarted += InternalHandleStarting;
        }

        private static void InternalHandleRestarting(RoundRestartEvent _)
        {
            NpcHandler.DestroyNpcs();
            RemoteAdminUtils.ClearObjects();
            ExRound.State = RoundState.Restarting;
            ExTeslaGate._pauseUpdate = true;
        }

        private static void InternalHandleWaiting(WaitingForPlayersEvent _)
        {
            ExPlayer._localPlayer = null;
            ExPlayer._hostPlayer = null;

            GhostModePatch.GhostedPlayers.Clear();
            GhostModePatch.GhostedTo.Clear();

            ExRound.RoundNumber++;
            ExRound.StartedAt = DateTime.MinValue;
            ExRound.State = RoundState.WaitingForPlayers;

            NavigationMesh.Reset();
            PrefabUtils.ReloadPrefabs();

            ProfilerMarker.LogAllMarkers(ExLoader.Loader.Config.Logging.ProfilingAsDebug);
            ProfilerMarker.ClearAllMarkers();
        }

        private static void InternalHandleStarting(RoundStartEvent _)
        {
            ExRound.StartedAt = DateTime.Now;
            ExRound.State = RoundState.InProgress;
        }

        internal static void InvokeWaiting(WaitingForPlayersEvent ev)
            => OnWaitingForPlayers?.Invoke(ev);

        internal static void InvokeStarted(RoundStartEvent ev)
            => OnRoundStarted?.Invoke(ev);

        internal static void InvokeEnded(RoundEndEvent ev)
            => OnRoundEnded?.Invoke(ev);

        internal static void InvokeRestarted(RoundRestartEvent ev)
            => OnRoundRestarted?.Invoke(ev);
    }
}