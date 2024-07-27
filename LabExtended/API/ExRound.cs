using Footprinting;

using GameCore;

using LabExtended.API.Enums;
using LabExtended.API.Internal;

using LabExtended.Core;
using LabExtended.Extensions;
using LightContainmentZoneDecontamination;
using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.RoleAssign;

using RoundRestarting;

using UnityEngine;

namespace LabExtended.API
{
    /// <summary>
    /// A class used for managing the round.
    /// </summary>
    public static class ExRound
    {
        private static RoundLock? _roundLockTracking;
        private static RoundLock? _lobbyLockTracking;

        /// <summary>
        /// Gets the round's number.
        /// </summary>
        public static short RoundNumber { get; internal set; }

        /// <summary>
        /// Gets the round's current state.
        /// </summary>
        public static RoundState State { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable the round lock if the player who enabled it leaves.
        /// </summary>
        public static bool DisableRoundLockOnLeave { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not to disable the lobby lock if the player who enabled it leaves.
        /// </summary>
        public static bool DisableLobbyLockOnLeave { get; set; }

        /// <summary>
        /// Gets a value indicating whether the round is ending.
        /// </summary>
        public static bool IsEnding => State is RoundState.Ending;

        /// <summary>
        /// Gets a value indicating whether the round has ended.
        /// </summary>
        public static bool IsEnded => State is RoundState.Ended;

        /// <summary>
        /// Gets a value indicating whether the round is in progress.
        /// </summary>
        public static bool IsRunning => State is RoundState.InProgress;

        /// <summary>
        /// Gets a value indicating whether the round is waiting for players.
        /// </summary>
        public static bool IsWaitingForPlayers => State is RoundState.WaitingForPlayers;

        /// <summary>
        /// Gets a value indicating whether or not SCP-079 was recontained.
        /// </summary>
        public static bool IsScp079Recontained { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether or not Light Containmetn Zone has started decontaminating.
        /// </summary>
        public static bool IsLczDecontaminated => DecontaminationController.Singleton?.IsDecontaminating ?? false;

        /// <summary>
        /// Gets the date of this round's start.
        /// </summary>
        public static DateTime StartedAt { get; internal set; }

        /// <summary>
        /// Gets the date of the round lock being enabled or <see cref="DateTime.MinValue"/> if the round lock is disabled.
        /// </summary>
        public static DateTime RoundLockEnabledAt => _roundLockTracking?.EnabledAt ?? DateTime.MinValue;

        /// <summary>
        /// Gets the date of the lobby lock being enabled or <see cref="DateTime.MinValue"/> if the lobby lock is disabled.
        /// </summary>
        public static DateTime LobbyLockEnabledAt => _lobbyLockTracking?.EnabledAt ?? DateTime.MinValue;

        /// <summary>
        /// Gets the amount of time that has passed since the round lock was enabled or <see cref="TimeSpan.Zero"/> if the round lock is disabled.
        /// </summary>
        public static TimeSpan TimePassedSinceRoundLock => _roundLockTracking.HasValue ? DateTime.Now - _roundLockTracking.Value.EnabledAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the amount of time that has passed since the lobby lock was enabled or <see cref="TimeSpan.Zero"/> if the lobby lock is disabled.
        /// </summary>
        public static TimeSpan TimePassedSinceLobbyLock => _lobbyLockTracking.HasValue ? DateTime.Now - _lobbyLockTracking.Value.EnabledAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the amount of time that has passed since the round started or <see cref="TimeSpan.Zero"/> if the round isn't running.
        /// </summary>
        public static TimeSpan Duration => IsRunning ? DateTime.Now - StartedAt : TimeSpan.Zero;

        /// <summary>
        /// Gets the player who enabled the round lock or <see langword="null"/> if the round lock is disabled.
        /// </summary>
        public static ExPlayer RoundLockEnabledBy => _roundLockTracking?.EnabledBy ?? null;

        /// <summary>
        /// Gets the player who enabled the lobby lock or <see langword="null"/> if the lobby lock is disabled.
        /// </summary>
        public static ExPlayer LobbyLockEnabledBy => _lobbyLockTracking?.EnabledBy ?? null;

        /// <summary>
        /// Gets the <see cref="Footprint"/> of the player who enabled the round lock.
        /// </summary>
        public static Footprint RoundLockEnabledByFootprint => _roundLockTracking?.EnabledByFootprint ?? default;

        /// <summary>
        /// Gets the <see cref="Footprint"/> of the player who enabled the lobby lock.
        /// </summary>
        public static Footprint LobbyLockEnabledByFootpring => _lobbyLockTracking?.EnabledByFootprint ?? default;

        /// <summary>
        /// Gets or sets the round lock tracker.
        /// </summary>
        public static RoundLock? RoundLock
        {
            get => _roundLockTracking;
            set
            {
                if (value.HasValue && value.Value.EnabledBy is null)
                    value = new RoundLock(ExPlayer.Host);

                _roundLockTracking = value;

                RoundSummary.RoundLock = _roundLockTracking.HasValue;

                if (_roundLockTracking.HasValue)
                    ExLoader.Debug("Round API", $"Round Lock enabled by &3{RoundLockEnabledBy?.Name ?? "null"}&r (&6{RoundLockEnabledBy?.UserId ?? null}&r)");
                else
                    ExLoader.Debug("Round API", $"Round Lock disabled.");
            }
        }

        /// <summary>
        /// Gets or sets the lobby lock tracker.
        /// </summary>
        public static RoundLock? LobbyLock
        {
            get => _lobbyLockTracking;
            set
            {
                if (value.HasValue && value.Value.EnabledBy is null)
                    value = new RoundLock(ExPlayer.Host);

                _lobbyLockTracking = value;

                RoundStart.LobbyLock = _lobbyLockTracking.HasValue;

                if (_lobbyLockTracking.HasValue)
                    ExLoader.Debug("Round API", $"Lobby Lock enabled by &3{LobbyLockEnabledBy?.Name ?? "null"}&r (&6{LobbyLockEnabledBy?.UserId ?? null}&r)");
                else
                    ExLoader.Debug("Round API", $"Lobby Lock disabled.");
            }
        }

        /// <summary>
        /// Gets or sets the forced decontamination status.
        /// </summary>
        public static DecontaminationController.DecontaminationStatus DecontaminationStatus
        {
            get => DecontaminationController.Singleton?.NetworkDecontaminationOverride ?? DecontaminationController.DecontaminationStatus.None;
            set => DecontaminationController.Singleton!.NetworkDecontaminationOverride = value;
        }

        /// <summary>
        /// Gets the decontamination phase.
        /// </summary>
        public static DecontaminationController.DecontaminationPhase.PhaseFunction DecontaminationPhase
        {
            get
            {
                if (DecontaminationController.Singleton._nextPhase < 0)
                    return DecontaminationController.DecontaminationPhase.PhaseFunction.None;

                return DecontaminationController.Singleton.DecontaminationPhases[DecontaminationController.Singleton._nextPhase].Function;
            }
        }

        /// <summary>
        /// Gets or sets the action to do when the round ends.
        /// </summary>
        public static ServerStatic.NextRoundAction RoundEndAction
        {
            get => ServerStatic.StopNextRound;
            set => ServerStatic.StopNextRound = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can end.
        /// </summary>
        public static bool IsRoundLocked
        {
            get => _roundLockTracking.HasValue || RoundSummary.RoundLock;
            set => _roundLockTracking = value ? new RoundLock(ExPlayer.Host) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can start.
        /// </summary>
        public static bool IsLobbyLocked
        {
            get => _lobbyLockTracking.HasValue || RoundStart.LobbyLock;
            set => _lobbyLockTracking = value ? new RoundLock(ExPlayer.Host) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should restart once the round ends.
        /// </summary>
        public static bool ShouldRestartOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Restart;
            set => RoundEndAction = value ? ServerStatic.NextRoundAction.Restart : (RoundEndAction is ServerStatic.NextRoundAction.Restart ? ServerStatic.NextRoundAction.DoNothing : ServerStatic.NextRoundAction.Shutdown);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should shut down once the round ends.
        /// </summary>
        public static bool ShouldShutdownOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Shutdown;
            set => RoundEndAction = value ? ServerStatic.NextRoundAction.Shutdown : (RoundEndAction is ServerStatic.NextRoundAction.Shutdown ? ServerStatic.NextRoundAction.DoNothing : ServerStatic.NextRoundAction.Restart);
        }

        /// <summary>
        /// Gets or sets the current's Chaos Insurgency target count.
        /// </summary>
        public static int ChaosTargetCount
        {
            get => RoundSummary.singleton?.Network_chaosTargetCount ?? 0;
            set => RoundSummary.singleton!.Network_chaosTargetCount = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped Class-Ds.
        /// </summary>
        public static int EscapedClassDs
        {
            get => RoundSummary.EscapedClassD;
            set => RoundSummary.EscapedClassD = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped scientists.
        /// </summary>
        public static int EscapedScientists
        {
            get => RoundSummary.EscapedScientists;
            set => RoundSummary.EscapedScientists = value;
        }

        /// <summary>
        /// Gets or sets the amount of total kills.
        /// </summary>
        public static int Kills
        {
            get => RoundSummary.Kills;
            set => RoundSummary.Kills = value;
        }

        /// <summary>
        /// Gets or sets the amount of SCPs that have survived the round.
        /// </summary>
        public static int SurvivingSCPs
        {
            get => RoundSummary.SurvivingSCPs;
            set => RoundSummary.SurvivingSCPs = value;
        }

        /// <summary>
        /// Gets or sets the amount of kills that the SCPs did.
        /// </summary>
        public static int KillsByScp
        {
            get => RoundSummary.KilledBySCPs;
            set => RoundSummary.KilledBySCPs = value;
        }

        /// <summary>
        /// Gets the amount of players that have been changed into zombies.
        /// </summary>
        public static int ChangedIntoZombies
        {
            get => RoundSummary.ChangedIntoZombies;
            set => RoundSummary.ChangedIntoZombies = value;
        }

        /// <summary>
        /// Forces the round to start.
        /// </summary>
        public static void ForceStart()
            => CharacterClassManager.ForceRoundStart();

        /// <summary>
        /// Forces the round to end.
        /// </summary>
        public static void ForceEnd()
            => RoundSummary.singleton?.ForceEnd();

        /// <summary>
        /// Restarts the round.
        /// </summary>
        /// <param name="fastRestart">Whether or not to use fast restart.</param>
        /// <param name="restartAction">The action to perform.</param>
        public static void Restart(bool fastRestart = true, ServerStatic.NextRoundAction? restartAction = null)
        {
            if (restartAction.HasValue)
                RoundEndAction = restartAction.Value;

            var oldValue = CustomNetworkManager.EnableFastRestart;

            CustomNetworkManager.EnableFastRestart = fastRestart;
            RoundRestart.InitiateRoundRestart();
            CustomNetworkManager.EnableFastRestart = oldValue;
        }

        /// <summary>
        /// Restarts the round silently (using fast restart).
        /// </summary>
        public static void RestartSilently()
            => Restart(true, ServerStatic.NextRoundAction.DoNothing);

        /// <summary>
        /// Generates round-start player role list.
        /// </summary>
        /// <returns>A dictionary of players and their roles.</returns>
        public static Dictionary<ExPlayer, RoleTypeId> ChooseRoles()
        {
            var roles = new Dictionary<ExPlayer, RoleTypeId>();

            try
            {
                var respawnQueue = ConfigFile.ServerConfig.GetString("team_respawn_queue", "4014314031441404134041434414");

                if (respawnQueue.Length != RoleAssigner._prevQueueSize)
                {
                    RoleAssigner._totalQueue = new Team[respawnQueue.Length];
                    RoleAssigner._humanQueue = new Team[respawnQueue.Length];
                    RoleAssigner._prevQueueSize = respawnQueue.Length;
                }

                ExLoader.Debug("Round API", $"Generating roles");

                var humanIndex = 0;
                var totalIndex = 0;

                for (int i = 0; i < respawnQueue.Length; i++)
                {
                    if (!int.TryParse(respawnQueue[i].ToString(), out var teamId) || teamId < 0 || teamId > byte.MaxValue || !Enum.IsDefined(typeof(Team), (byte)teamId))
                        continue;

                    var team = (Team)teamId;

                    if (team != Team.SCPs)
                        RoleAssigner._humanQueue[humanIndex++] = team;

                    RoleAssigner._totalQueue[totalIndex++] = team;
                }

                RoleAssigner._spawned = true;
                RoleAssigner.LateJoinTimer.Restart();

                var players = ListPool<ExPlayer>.Shared.Rent(ExPlayer._players.Where(p => RoleAssigner.CheckPlayer(p.Hub)));
                var scps = 0;

                for (int i = 0; i < roles.Count; i++)
                {
                    if (RoleAssigner._totalQueue[i % totalIndex] is Team.SCPs)
                        scps++;
                }

                ExLoader.Debug("Round API", $"SCPs to generate: {scps}");

                DecideScps(players, roles, scps);
                DecideHumans(players, roles, RoleAssigner._humanQueue, humanIndex);

                ExLoader.Debug("Round API", $"Generated {roles.Count} roles");
            }
            catch (Exception ex)
            {
                ExLoader.Error("Round API", $"Failed to generate roles!\n{ex.ToColoredString()}");
            }

            return roles;

            #region Helper Methods
            void DecideHumans(List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, Team[] queue, int size)
            {
                HumanSpawner._humanQueue = queue;

                HumanSpawner._queueClock = 0;
                HumanSpawner._queueLength = size;

                var candidates = ListPool<ExPlayer>.Shared.Rent();
                var array = new RoleTypeId[players.Count];

                for (int i = 0; i < players.Count; i++)
                    array[i] = HumanSpawner.NextHumanRoleToSpawn;

                array.ShuffleList();

                ExLoader.Debug("Round API", $"Generating human roles");

                for (int i = 0; i < players.Count; i++)
                {
                    candidates.Clear();

                    var num = int.MaxValue;
                    var role = array[i];

                    foreach (var player in ExPlayer.Players)
                    {
                        if (!player.IsVerified || roles.ContainsKey(player))
                            continue;

                        var roleHistory = HumanSpawner.History.GetOrAdd(player.UserId, () => new HumanSpawner.RoleHistory());
                        var num2 = 0;

                        for (int x = 0; x < 5; x++)
                        {
                            if (roleHistory.History[x] == role)
                                num2++;
                        }

                        if (num2 <= num)
                        {
                            if (num2 < num)
                                candidates.Clear();

                            candidates.Add(player);
                            num = num2;

                            ExLoader.Debug("Round API", $"Chosen candidate {player.Name}");
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        var random = candidates.RandomItem();
                        roles[random] = role;
                        ExLoader.Debug("Round API", $"Assigned role of {random.Name} to {role}");
                    }
                }

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void DecideScps(List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, int scps)
            {
                ExLoader.Debug("Round API", $"Generating SCP roles");

                ScpSpawner.EnqueuedScps.Clear();

                for (int i = 0; i < scps; i++)
                    ScpSpawner.EnqueuedScps.Add(ScpSpawner.NextScp);

                var candidates = ListPool<ExPlayer>.Shared.Rent();

                ExLoader.Debug("Round API", $"Generating list");

                GenerateScpList(candidates, players, scps);

                ExLoader.Debug("Round API", $"Choosing players");

                while (ScpSpawner.EnqueuedScps.Count > 0)
                    ChooseScps(candidates, players, roles);

                ExLoader.Debug("Round API", $"Selected {roles.Count} SCPs");

                players.RemoveAll(p => roles.ContainsKey(p));

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void GenerateScpList(List<ExPlayer> candidates, List<ExPlayer> players, int scps)
            {
                using (var ticketLoader = new ScpTicketsLoader())
                {
                    if (scps <= 0)
                        return;

                    var num = 0;

                    foreach (var player in players)
                    {
                        var tickets = ticketLoader.GetTickets(player.Hub, 10);

                        if (tickets >= num)
                        {
                            if (tickets > num)
                                candidates.Clear();

                            num = tickets;
                            candidates.Add(player);

                            ExLoader.Debug("Round API", $"Added SCP candidate {player.Name}");
                        }
                    }

                    if (candidates.Count > 1)
                    {
                        var randomPlayer = candidates.RandomItem();

                        candidates.Clear();
                        candidates.Add(randomPlayer);

                        ExLoader.Debug("Round API", $"Selected random candidate: {randomPlayer.Name}");
                    }

                    scps -= candidates.Count;

                    if (scps <= 0)
                        return;

                    var potential = ListPool<ScpPlayerPicker.PotentialScp>.Shared.Rent();
                    var num2 = 0L;

                    foreach (var player in players)
                    {
                        if (!candidates.Contains(player))
                        {
                            var num3 = 1L;
                            var tickets = ticketLoader.GetTickets(player.Hub, 10);

                            for (int i = 0; i < scps; i++)
                                num3 *= tickets;

                            potential.Add(new ScpPlayerPicker.PotentialScp { Player = player.Hub, Weight = num3 });
                            num2 += num3;

                            ExLoader.Debug("Round API", $"Added potential SCP {player.Name} ({num3})");
                        }
                    }

                    while (scps > 0)
                    {
                        var num4 = (double)UnityEngine.Random.value * num2;

                        for (int i = 0; i < potential.Count; i++)
                        {
                            var scp = potential[i];

                            num4 -= scp.Weight;

                            if (num4 < 0)
                            {
                                scps--;
                                candidates.Add(ExPlayer.Get(scp.Player));
                                potential.RemoveAt(i);
                                num2 -= scp.Weight;
                                ExLoader.Debug("Round API", $"Added candidate {scp.Player.nicknameSync.MyNick}");
                                break;
                            }
                        }
                    }

                    ExLoader.Debug("Round API", $"Found {candidates.Count} candidates");

                    ListPool<ScpPlayerPicker.PotentialScp>.Shared.Return(potential);
                }
            }

            void ChooseScps(List<ExPlayer> candidates, List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles)
            {
                var scpRole = ScpSpawner.EnqueuedScps[0];

                ExLoader.Debug("Round API", $"Spawning SCP: {scpRole}");

                ScpSpawner.EnqueuedScps.RemoveAt(0);
                ScpSpawner.ChancesBuffer.Clear();

                var num = 1;
                var num2 = 0;

                foreach (var player in candidates)
                {
                    ExLoader.Debug("Round API", $"Checking candidate: {player.Name}");

                    var num3 = ScpSpawner.GetPreferenceOfPlayer(player.Hub, scpRole);

                    foreach (var otherScp in ScpSpawner.EnqueuedScps)
                        num3 -= ScpSpawner.GetPreferenceOfPlayer(player.Hub, otherScp);

                    ExLoader.Debug("Round API", $"Weight: {num3}");

                    num2++;

                    ScpSpawner.ChancesBuffer[player.Hub] = num3;
                    num = Mathf.Min(num3, num);
                }

                var num4 = 0f;

                ScpSpawner.SelectedSpawnChances.Clear();

                foreach (var pair in ScpSpawner.ChancesBuffer)
                {
                    var num5 = Mathf.Pow(pair.Value - num + 1f, num2);
                    ScpSpawner.SelectedSpawnChances[pair.Key] = num5;
                    ExLoader.Debug("Round API", $"{pair.Key.nicknameSync.MyNick} weight: {num5}");
                    num4 += num5;
                }

                var num6 = num4 * UnityEngine.Random.value;
                var num7 = 0f;

                foreach (var pair in ScpSpawner.SelectedSpawnChances)
                {
                    num7 += pair.Value;

                    if (num7 > num6)
                    {
                        candidates.RemoveAll(p => p.Hub == pair.Key);
                        players.RemoveAll(p => p.Hub == pair.Key);
                        roles[ExPlayer.Get(pair.Key)] = scpRole;
                        ExLoader.Debug("Round API", $"Assigned SCP {scpRole} to {pair.Key.nicknameSync.MyNick}");
                    }
                }
            }
            #endregion
        }
    }
}