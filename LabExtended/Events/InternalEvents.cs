using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.Voice;
using LabExtended.API.Hints;
using LabExtended.API.Modules;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Collections;
using LabExtended.API.CustomModules;

using LabExtended.Core;
using LabExtended.Core.Ticking;
using LabExtended.Core.Ticking.Timers;
using LabExtended.Core.Networking.Synchronization.Position;

using LabExtended.Events.Player;

using LabExtended.Commands;

using NorthwoodLib.Pools;

using PluginAPI.Events;

using NetworkManagerUtils;

namespace LabExtended.Events
{
    internal static class InternalEvents
    {
        internal static void InternalHandleRoundWaiting()
        {
            ExMap.GenerateMap();
            ExPlayer._preauthData.Clear();

            if (ExPlayer._hostPlayer != null)
            {
                ExPlayer._hostPlayer.StopModule();
                ExPlayer._hostPlayer = null;
            }

            DamageInfo._wrappers.Clear();    

            ExRound.RoundNumber++;

            ExRound.IsScp079Recontained = false;

            ExRound.StartedAt = DateTime.MinValue;
            ExRound.State = RoundState.WaitingForPlayers;

            // No reason not to reset the NPC connection ID
            DummyNetworkConnection._idGenerator = 65535;
        }

        internal static void InternalHandleRoundRestart()
        {
            if (ExTeslaGate._tickHandle.IsActive)
                ExTeslaGate._tickHandle.IsPaused = true;

            try
            {
                ExMap._gates.Clear();
            }
            catch { }

            ExRound.State = RoundState.Restarting;
        }

        internal static void InternalHandleRoundStart()
        {
            ExRound.StartedAt = DateTime.Now;
            ExRound.State = RoundState.InProgress;

            if (ExTeslaGate._tickHandle.IsPaused)
                ExTeslaGate._tickHandle.IsPaused = false;
            else
                ExTeslaGate._tickHandle = ApiLoader.ApiConfig.TickSection.GetCustomOrDefault("TeslaGates", TickDistribution.UnityTick).CreateHandle(TickDistribution.CreateWith(ExTeslaGate.TickGates, null, new DynamicTickTimer(() => ExTeslaGate.TickRate / TimeSpan.TicksPerMillisecond)));
        }

        internal static void InternalHandlePlayerAuth(PlayerPreauthEvent ev)
        {
            ExPlayer._preauthData[ev.UserId] = ev.Region;
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

                        ApiLog.Debug("Modules API", $"Re-added transient module &3{type.Name}&r (&6{module.ModuleId}&r) to player &3{player.Name}&r (&6{player.UserId}&r)!");
                    }
                    else
                    {
                        ApiLog.Warn("Extended API", $"Could not add transient module &3{type.Name}&r to player &3{player.Name}&r (&6{player.UserId}&r) - active instance found.");
                    }
                }
            }

            player._voice = player.AddModule<VoiceModule>();
            player._raModule = player.AddModule<RemoteAdminModule>();

            if (player._modules.TryGetValue(typeof(PlayerStorageModule), out var storageModule))
                player._storage = (PlayerStorageModule)storageModule;
            else
                player._storage = player.AddModule<PlayerStorageModule>();

            if (!ExPlayer._allPlayers.Contains(player))
            {
                ExPlayer._allPlayers.Add(player);

                if (player.IsNpc)
                    ExPlayer._npcPlayers.Add(player);
                else
                    ExPlayer._allPlayers.Add(player);
            }

            if (!player.IsNpc && !player.IsServer)
                HintController.OnJoined(player);

            ApiLog.Info("LabExtended", $"Player &3{player.Name}&r (&6{player.UserId}&r) &2joined&r from &3{player.Address} ({player.CountryCode})&r!");
        }

        internal static void InternalHandlePlayerLeave(ExPlayer player)
        {
            if (ExRound.State is RoundState.InProgress || ExRound.State is RoundState.WaitingForPlayers)
            {
                if (ApiLoader.BaseConfig.DisableRoundLockOnLeave && ExRound.RoundLock.HasValue && ExRound.RoundLock.Value.EnabledBy == player)
                {
                    ExRound.IsRoundLocked = false;
                    ApiLog.Warn("Round API", $"Round Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
                }

                if (ApiLoader.BaseConfig.DisableLobbyLockOnLeave && ExRound.LobbyLock.HasValue && ExRound.LobbyLock.Value.EnabledBy == player)
                {
                    ExRound.IsLobbyLocked = false;
                    ApiLog.Warn("Round API", $"Lobby Lock disabled - the player who enabled it (&3{player.Name}&r &6{player.UserId}&r) left the server.");
                }
            }

            if (ExPlayer._hostPlayer != null && ExPlayer._hostPlayer == player)
                ExPlayer._hostPlayer = null;

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

            player.StopModule();

            ExPlayer._allPlayers.Remove(player);

            if (player.IsNpc)
                ExPlayer._npcPlayers.Remove(player);
            else
                ExPlayer._players.Remove(player);

            ApiLog.Info("LabExtended", $"Player &3{player.Name}&r (&3{player.UserId}&r) &1left&r from &3{player.Address}&r!");

            if (!player.IsNpc && !player.IsServer)
                HintController.OnLeft(player);
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

            if (args.Player is null || args.Player.Voice is null)
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
    }
}