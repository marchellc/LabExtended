using LabExtended.API;
using LabExtended.API.Npcs;
using LabExtended.API.Npcs.Navigation;
using LabExtended.API.Enums;
using LabExtended.API.Voice;
using LabExtended.API.Hints;
using LabExtended.API.Modules;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Collections;

using LabExtended.Events.Player;

using LabExtended.Core;
using LabExtended.Core.Ticking;
using LabExtended.Core.Profiling;

using LabExtended.Commands;
using LabExtended.Patches.Functions;

using NorthwoodLib.Pools;

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static void InternalHandleRoundEnd()
        {

        }

        internal static void InternalHandleRoundWaiting()
        {
            if (ExPlayer._hostPlayer != null)
            {
                ExPlayer._hostPlayer.StopModule();
                ExPlayer._hostPlayer = null;
            }

            if (ReferenceHub.TryGetHostHub(out var hostHub))
                ExPlayer._hostPlayer = new ExPlayer(hostHub);
            else
                ExLoader.Warn("Player API", $"Failed to fetch the host player.");

            GhostModePatch.GhostedPlayers.Clear();
            GhostModePatch.GhostedTo.Clear();

            ExRound.RoundNumber++;
            ExRound.IsScp079Recontained = false;
            ExRound.StartedAt = DateTime.MinValue;
            ExRound.State = RoundState.WaitingForPlayers;

            NavigationMesh.Reset();
            Prefabs.ReloadPrefabs();

            ProfilerMarker.LogAllMarkers(ExLoader.Loader.Config.Logging.ProfilingAsDebug);

            foreach (var marker in ProfilerMarker.AllMarkers)
            {
                if (marker.Frames.Count() >= 50)
                    marker.Clear();
            }
        }

        internal static void InternalHandleRoundRestart()
        {
            TickManager.PauseTick("Tesla Gate Update");

            try
            {
                API.ExTeslaGate._wrappers.Clear();
            }
            catch { }

            NpcHandler.DestroyNpcs();

            ExRound.State = RoundState.Restarting;
        }

        internal static void InternalHandleRoundStart()
        {
            ExRound.StartedAt = DateTime.Now;
            ExRound.State = RoundState.InProgress;

            if (TickManager.IsPaused("Tesla Gate Update"))
                TickManager.ResumeTick("Tesla Gate Update");
        }

        internal static void InternalHandlePlayerJoin(ExPlayer player)
        {
            if (TransientModule._cachedModules.TryGetValue(player.UserId, out var transientModules))
            {
                foreach (var module in transientModules)
                {
                    var type = module.GetType();

                    if (!player._modules.ContainsKey(type))
                    {
                        player._modules[type] = module;

                        module.Parent = player;
                        module.StartModule();

                        player.OnModuleAdded(module);

                        ExLoader.Debug("Modules API", $"Re-added transient module &3{type.Name}&r (&6{module.ModuleId}&r) to player &3{player.Name}&r (&6{player.UserId}&r)!");
                    }
                    else
                    {
                        ExLoader.Warn("Extended API", $"Could not add transient module &3{type.Name}&r to player &3{player.Name}&r (&6{player.UserId}&r) - active instance found.");
                    }
                }
            }

            player._hints = player.AddModule<HintModule>();
            player._voice = player.AddModule<VoiceModule>();
            player._raModule = player.AddModule<RemoteAdminModule>();

            if (player._modules.TryGetValue(typeof(PlayerStorageModule), out var storageModule))
                player._storage = (PlayerStorageModule)storageModule;
            else
                player._storage = player.AddModule<PlayerStorageModule>();

            ExPlayer._players.Add(player);
            ExPlayer._allPlayers.Add(player);

            ExLoader.Info("Extended API", $"Player &3{player.Name}&r (&6{player.UserId}&r) &2joined&r from &3{player.Address}&r!");
        }

        internal static void InternalHandlePlayerLeave(ExPlayer player)
        {
            if (ExRound.DisableRoundLockOnLeave && ExRound.RoundLock.HasValue && ExRound.RoundLock.Value.EnabledBy == player)
            {
                ExRound.IsRoundLocked = false;
                ExLoader.Warn("Round API", $"Round Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
            }

            if (ExRound.DisableLobbyLockOnLeave && ExRound.LobbyLock.HasValue && ExRound.LobbyLock.Value.EnabledBy == player)
            {
                ExRound.IsLobbyLocked = false;
                ExLoader.Warn("Round API", $"Lobby Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
            }

            GhostModePatch.GhostedPlayers.Remove(player.NetId);
            GhostModePatch.GhostedTo.Remove(player.NetId);

            CustomCommand._continuedContexts.Remove(player.NetId);

            foreach (var pair in GhostModePatch.GhostedTo)
                pair.Value.Remove(player.NetId);

            foreach (var helper in PlayerCollection._handlers)
                helper.Remove(player.NetId);

            ExPlayer._allPlayers.Remove(player);

            player.StopModule();

            if (!player.IsNpc)
            {
                ExPlayer._players.Remove(player);
                ExLoader.Info("Extended API", $"Player &3{player.Name}&r (&3{player.UserId}&r) &1left&r from &3{player.Address}&r!");
            }
            else
            {
                if (!player.NpcHandler._isDestroying)
                    player.NpcHandler.Destroy();

                ExPlayer._npcPlayers.Remove(player);
            }
        }

        internal static void InternalHandleRoleChange(PlayerSpawningArgs args)
        {
            if (args.Player is null || args.Player.IsNpc || args.Player.Voice is null)
                return;

            var profilesToRemove = ListPool<VoiceProfile>.Shared.Rent();

            foreach (var profile in args.Player.Voice.Profiles)
            {
                if (!profile.OnRoleChanged(args.NewRole))
                    profilesToRemove.Add(profile);
            }

            foreach (var profile in profilesToRemove)
                args.Player.Voice.RemoveProfile(profile);

            ListPool<VoiceProfile>.Shared.Return(profilesToRemove);
        }

        internal static void InternalHandleMapGenerated()
        {
            ExMap.GenerateMap();
        }
    }
}