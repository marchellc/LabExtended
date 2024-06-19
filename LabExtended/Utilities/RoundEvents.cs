using LabExtended.API;
using LabExtended.API.Npcs;
using LabExtended.API.Npcs.Navigation;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Round;

using LabExtended.Core.Hooking;
using LabExtended.Patches.Functions;

using PluginAPI.Events;

namespace LabExtended.Utilities
{
    public static class RoundEvents
    {
        [HookIgnore]
        public static event Action<RoundEndEvent> OnRoundEnded;

        [HookIgnore]
        public static event Action<RoundStartEvent> OnRoundStarted;

        [HookIgnore]
        public static event Action<RoundRestartEvent> OnRoundRestarted;

        [HookIgnore]
        public static event Action<WaitingForPlayersEvent> OnWaitingForPlayers;

        static RoundEvents()
        {
            OnWaitingForPlayers += HandleWaiting;
            OnRoundRestarted += HandleRestarting;
            OnRoundStarted += HandleStarting;
        }

        private static void HandleRestarting(RoundRestartEvent _)
        {
            NpcHandler.DestroyNpcs();
            RemoteAdminUtils.ClearObjects();
        }

        private static void HandleWaiting(WaitingForPlayersEvent _)
        {
            ExPlayer._localPlayer = null;
            ExPlayer._hostPlayer = null;

            GhostModePatch.GhostedPlayers.Clear();
            GhostModePatch.GhostedTo.Clear();

            ExRound.RoundNumber++;
            ExRound.StartedAt = DateTime.MinValue;

            NavigationMesh.Reset();
        }

        private static void HandleStarting(RoundStartEvent _)
        {
            ExRound.StartedAt = DateTime.Now;
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