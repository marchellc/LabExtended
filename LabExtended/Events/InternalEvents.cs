using LabExtended.API;
using LabExtended.API.Npcs;
using LabExtended.API.Npcs.Navigation;

using LabExtended.API.Enums;
using LabExtended.API.Voice;
using LabExtended.API.Hints;
using LabExtended.API.Modules;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Collections;
using LabExtended.API.CustomModules;

using LabExtended.Core;
using LabExtended.Core.Ticking;
using LabExtended.Core.Profiling;

using LabExtended.Events.Player;
using LabExtended.Patches.Fixes;
using LabExtended.Commands;

using NorthwoodLib.Pools;
using MapGeneration.Distributors;
using static Mirror.LiteNetLib4Mirror.LiteNetLib4MirrorCore;
using LabExtended.Core.Synchronization.Position;

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static void InternalHandleRoundWaiting()
        {
            if (ExPlayer._hostPlayer != null)
            {
                ExPlayer._hostPlayer.StopModule();
                ExPlayer._hostPlayer = null;
            }

            NetIdWaypointIgnoreDoorsPatch.CustomWaypoints.Clear();
            NetIdWaypointIgnoreDoorsPatch.DisabledWaypoints.Clear();

            if (ReferenceHub.TryGetHostHub(out var hostHub))
                ExPlayer._hostPlayer = new ExPlayer(hostHub);
            else
                ApiLoader.Warn("Player API", $"Failed to fetch the host player.");

            ExRound.RoundNumber++;

            ExRound.IsScp079Recontained = false;

            ExRound.StartedAt = DateTime.MinValue;
            ExRound.State = RoundState.WaitingForPlayers;

            NavigationMesh.Reset();
            Prefabs.ReloadPrefabs();

            if (ApiLoader.Config.ApiOptions.PerformanceOptions.EnableProfilerLogs)
                ProfilerMarker.LogAllMarkers(ApiLoader.Config.LogOptions.ProfilingAsDebug);
        }

        internal static void InternalHandleRoundRestart()
        {
            TickManager.PauseTick("Tesla Gate Update");

            try
            {
                ExMap._gates.Clear();
            }
            catch { }

            NpcHandler.DestroyNpcs();
            Prefabs.DestroySpawnedDoors();

            ExRound.State = RoundState.Restarting;
        }

        internal static void InternalHandleRoundStart()
        {
            ExRound.StartedAt = DateTime.Now;
            ExRound.State = RoundState.InProgress;

            if (TickManager.IsPaused("Tesla Gate Update"))
                TickManager.ResumeTick("Tesla Gate Update");

            ApiLoader.Info("Map API", $"Finished populating objects, cache state:\n" +
                $"Generators | {ExMap.Generators.Count}\n" +
                $"Elevators  | {ExMap.Elevators.Count}\n" +
                $"Airlock    | {ExMap.Airlocks.Count}\n" +
                $"Lockers    | {ExMap.Lockers.Count}\n" +
                $"Camera     | {ExMap.Cameras.Count}\n" +
                $"Doors      | {ExMap.Doors.Count}\n" +
                $"Gates      | {ExMap.TeslaGates.Count}\n" +
                $"Toys       | {ExMap.Toys.Count}");
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

                        ApiLoader.Debug("Modules API", $"Re-added transient module &3{type.Name}&r (&6{module.ModuleId}&r) to player &3{player.Name}&r (&6{player.UserId}&r)!");
                    }
                    else
                    {
                        ApiLoader.Warn("Extended API", $"Could not add transient module &3{type.Name}&r to player &3{player.Name}&r (&6{player.UserId}&r) - active instance found.");
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

            ApiLoader.Info("LabExtended", $"Player &3{player.Name}&r (&6{player.UserId}&r) &2joined&r from &3{player.Address}&r!");
        }

        internal static void InternalHandlePlayerLeave(ExPlayer player)
        {
            if (ExRound.State is RoundState.InProgress || ExRound.State is RoundState.WaitingForPlayers)
            {
                if (ApiLoader.Config.RoundOptions.DisableRoundLockOnLeave && ExRound.RoundLock.HasValue && ExRound.RoundLock.Value.EnabledBy == player)
                {
                    ExRound.IsRoundLocked = false;
                    ApiLoader.Warn("Round API", $"Round Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
                }

                if (ApiLoader.Config.RoundOptions.DisableLobbyLockOnLeave && ExRound.LobbyLock.HasValue && ExRound.LobbyLock.Value.EnabledBy == player)
                {
                    ExRound.IsLobbyLocked = false;
                    ApiLoader.Warn("Round API", $"Lobby Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
                }
            }

            player._sentRoles.Clear();
            player._invisibility.Clear();

            player.Inventory._droppedItems.Clear();

            CustomCommand._continuedContexts.Remove(player.NetId);

            foreach (var other in ExPlayer._allPlayers)
            {
                other._sentRoles.Remove(player.PlayerId);
                other._invisibility.Remove(player);
            }

            foreach (var helper in PlayerCollection.m_Handlers)
                helper.Remove(player.NetId);

            ExPlayer._allPlayers.Remove(player);

            player.StopModule();

            if (!player.IsNpc)
            {
                ExPlayer._players.Remove(player);
                ApiLoader.Info("LabExtended", $"Player &3{player.Name}&r (&3{player.UserId}&r) &1left&r from &3{player.Address}&r!");
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
            if (args.Player != null)
            {
                PositionSynchronizer._syncCache.Remove(args.Player);

                foreach (var player in ExPlayer._allPlayers)
                {
                    if (PositionSynchronizer._syncCache.TryGetValue(player, out var cache))
                        cache.Remove(args.Player);
                }
            }

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