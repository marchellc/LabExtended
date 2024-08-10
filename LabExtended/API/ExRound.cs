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
                if (!value.HasValue && !_roundLockTracking.HasValue)
                    return;

                if (value.HasValue && value.Value.EnabledBy is null)
                    value = new RoundLock(ExPlayer.Host);

                if (value.HasValue && _roundLockTracking.HasValue && _roundLockTracking.Value.EnabledBy == value.Value.EnabledBy)
                    return;

                _roundLockTracking = value;

                RoundSummary.RoundLock = _roundLockTracking.HasValue;

                if (_roundLockTracking.HasValue)
                    ApiLoader.Debug("Round API", $"Round Lock enabled by &3{RoundLockEnabledBy?.Name ?? "null"}&r (&6{RoundLockEnabledBy?.UserId ?? null}&r)");
                else
                    ApiLoader.Debug("Round API", $"Round Lock disabled.");
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
                if (!value.HasValue && !_lobbyLockTracking.HasValue)
                    return;

                if (value.HasValue && value.Value.EnabledBy is null)
                    value = new RoundLock(ExPlayer.Host);

                if (value.HasValue && _lobbyLockTracking.HasValue && _lobbyLockTracking.Value.EnabledBy == value.Value.EnabledBy)
                    return;

                _lobbyLockTracking = value;

                RoundStart.LobbyLock = _lobbyLockTracking.HasValue;

                if (_lobbyLockTracking.HasValue)
                    ApiLoader.Debug("Round API", $"Lobby Lock enabled by &3{LobbyLockEnabledBy?.Name ?? "null"}&r (&6{LobbyLockEnabledBy?.UserId ?? null}&r)");
                else
                    ApiLoader.Debug("Round API", $"Lobby Lock disabled.");
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
            set => RoundLock = value ? new RoundLock(ExPlayer.Host) : null;
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can start.
        /// </summary>
        public static bool IsLobbyLocked
        {
            get => _lobbyLockTracking.HasValue || RoundStart.LobbyLock;
            set => LobbyLock = value ? new RoundLock(ExPlayer.Host) : null;
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

                ApiLoader.Debug($"RoleGeneration", $"respawnQueue={respawnQueue}");

                if (respawnQueue.Length != RoleAssigner._prevQueueSize)
                {
                    RoleAssigner._totalQueue = new Team[respawnQueue.Length];
                    RoleAssigner._humanQueue = new Team[respawnQueue.Length];

                    RoleAssigner._prevQueueSize = respawnQueue.Length;
                }

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

                var players = new List<ExPlayer>(ExPlayer._allPlayers.Where(x => !x.IsServer && !x.IsNpc && x.IsOnline && x.Role.Is(RoleTypeId.None)));
                var scps = 0;

                for (int i = 0; i < players.Count; i++)
                {
                    if (RoleAssigner._totalQueue[i % totalIndex] is Team.SCPs)
                        scps++;
                }

                ApiLoader.Debug($"RoleGeneration", $"players={players.Count} scps={scps}");

                if (scps > 0)
                {
                    ApiLoader.Debug($"RoleGeneration", $"Assigning SCPs");

                    DecideScps(players, roles, scps);

                    players.RemoveAll(roles.ContainsKey);
                }

                if (players.Count > 0)
                {
                    ApiLoader.Debug($"RoleGeneration", $"Assigning humans");
                    DecideHumans(players, roles, RoleAssigner._humanQueue, humanIndex);
                }

                ApiLoader.Debug($"RoleGeneration", $"Finished role assignment");
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Round API", $"Failed to generate roles!\n{ex.ToColoredString()}");
            }

            return roles;

            #region Helper Methods
            void DecideHumans(List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, Team[] queue, int size)
            {
                ApiLoader.Debug($"RoleGeneration.DecideHumans", $"players={players.Count} roles={roles.Count} queue={queue.Length} size={size}");

                HumanSpawner._humanQueue = queue;

                HumanSpawner._queueClock = 0;
                HumanSpawner._queueLength = size;

                var candidates = ListPool<ExPlayer>.Shared.Rent();
                var array = new RoleTypeId[players.Count];

                ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Choosing {array.Length} roles");

                for (int i = 0; i < players.Count; i++)
                {
                    array[i] = HumanSpawner.NextHumanRoleToSpawn;
                    ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Chosen role i={i} role={array[i]}");
                }

                array.ShuffleList();

                for (int i = 0; i < players.Count; i++)
                {
                    candidates.Clear();

                    var num = int.MaxValue;
                    var role = array[i];

                    ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Choosing player for role i={i} role={role}");

                    foreach (var player in players)
                    {
                        if (roles.ContainsKey(player))
                            continue;

                        var roleHistory = HumanSpawner.History.GetOrAdd(player.UserId, () => new HumanSpawner.RoleHistory());
                        var num2 = 0;

                        for (int x = 0; x < 5; x++)
                        {
                            if (roleHistory.History[x] == role)
                            {
                                num2++;
                                ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Found role in player {player.Name} history, num2={num2}");
                            }
                        }

                        if (num2 <= num)
                        {
                            ApiLoader.Debug($"RoleGeneration.DecideHumans", $"num2={num2} num={num}");

                            if (num2 < num)
                            {
                                candidates.Clear();
                                ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Cleared candidates");
                            }

                            candidates.Add(player);
                            num = num2;

                            ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Added candidate {player.Name} num={num}");
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        var random = candidates.RandomItem();

                        roles[random] = role;

                        ApiLoader.Debug($"RoleGeneration.DecideHumans", $"Set role of player {random.Name} to {role}");
                    }
                    else
                    {
                        ApiLoader.Warn($"RoleGeneration.DecideHumans", $"No human role candidates were found.");
                    }
                }

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void DecideScps(List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, int scps)
            {
                ApiLoader.Debug($"RoleGeneration.DecideScps", $"players={players.Count} roles={roles.Count} scps={scps}");

                var candidates = ListPool<ExPlayer>.Shared.Rent();

                ApiLoader.Debug($"RoleGeneration.DecideScps", $"Generating candidates");

                GenerateScpList(candidates, roles, players, scps);

                var scpQueue = new Queue<RoleTypeId>();

                for (int i = 0; i < scps; i++)
                {
                    var nextScp = ScpSpawner.NextScp;

                    if (!scpQueue.Contains(nextScp))
                    {
                        ApiLoader.Debug($"RoleGeneration.DecideScps", $"i={i} nextScp={nextScp}");
                        scpQueue.Enqueue(nextScp);
                    }
                    else
                    {
                        ApiLoader.Warn($"RoleGeneration.DecideScps", $"Attempted to add a duplicate SCP role! (i={i} nextScp={nextScp})");
                    }
                }

                ApiLoader.Debug($"RoleGeneration.DecideScps", $"candidates={candidates.Count}");

                using (var ticketLoader = new ScpTicketsLoader())
                {
                    foreach (var player in ExPlayer.Players)
                    {
                        if (!RoleAssigner.CheckPlayer(player.Hub))
                            continue;

                        var tickets = ticketLoader.GetTickets(player.Hub, 10);

                        ticketLoader.ModifyTickets(player.Hub, tickets + 2);
                    }

                    foreach (var player in candidates)
                        ticketLoader.ModifyTickets(player.Hub, 10);
                }

                while (scpQueue.TryDequeue(out var scpRole))
                    ChooseScps(candidates, players, roles, scpRole, scpQueue);

                ApiLoader.Debug($"RoleGeneration.DecideScps", $"Finished, players={players.Count}");

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void GenerateScpList(List<ExPlayer> candidates, Dictionary<ExPlayer, RoleTypeId> roles, List<ExPlayer> players, int scps)
            {
                ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Generating candidates");

                if (scps <= 0)
                    return;

                using (var ticketLoader = new ScpTicketsLoader())
                {
                    var num = 0;

                    foreach (var player in players)
                    {
                        var tickets = ticketLoader.GetTickets(player.Hub, 10);

                        ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"player={player.Name} tickets={tickets}");

                        if (tickets >= num)
                        {
                            if (tickets > num)
                            {
                                candidates.Clear();
                                ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Found new max tickets, cleared candidates");
                            }

                            num = tickets;
                            candidates.Add(player);

                            ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Added candidate {player.Name}");
                        }
                    }

                    if (candidates.Count > 1)
                    {
                        var count = candidates.Count;
                        var randomPlayer = candidates.RandomItem();

                        candidates.Clear();
                        candidates.Add(randomPlayer);

                        ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Chosen random candidate {randomPlayer.Name} from {count} candidates");
                    }

                    scps -= candidates.Count;

                    ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"scps={scps}");

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

                            ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Found potential candidate {player.Name}, weight {num3}");

                            potential.Add(new ScpPlayerPicker.PotentialScp { Player = player.Hub, Weight = num3 });
                            num2 += num3;

                            ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"num2={num2} num3={num3}");
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
                                break;
                            }
                        }
                    }

                    ListPool<ScpPlayerPicker.PotentialScp>.Shared.Return(potential);
                }
            }

            void ChooseScps(List<ExPlayer> candidates, List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, RoleTypeId scpRole, Queue<RoleTypeId> scpQueue)
            {
                ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Choosing scps, candidates={candidates.Count} players={players.Count} roles={roles.Count} scpRole={scpRole}");

                ScpSpawner.ChancesBuffer.Clear();

                var num = 1;
                var num2 = 0;

                foreach (var player in candidates)
                {
                    var num3 = ScpSpawner.GetPreferenceOfPlayer(player.Hub, scpRole);

                    ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Preference of {player.Name}: {num3}");

                    foreach (var otherScp in scpQueue)
                    {
                        num3 -= ScpSpawner.GetPreferenceOfPlayer(player.Hub, otherScp);
                        ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Preference now {num3}");
                    }

                    num2++;

                    ScpSpawner.ChancesBuffer[player.Hub] = num3;
                    num = Mathf.Min(num3, num);

                    ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Chance of {player.Name} is {num3} (num2={num2}, num={num})");
                }

                var num4 = 0f;

                ScpSpawner.SelectedSpawnChances.Clear();

                foreach (var pair in ScpSpawner.ChancesBuffer)
                {
                    var num5 = Mathf.Pow(pair.Value - num + 1f, num2);

                    ScpSpawner.SelectedSpawnChances[pair.Key] = num5;

                    num4 += num5;

                    ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Spawn chance of {pair.Key.nicknameSync.MyNick} is now {num5} (num4={num4})");
                }

                var num6 = num4 * UnityEngine.Random.value;
                var num7 = 0f;

                ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"num6={num6}");

                foreach (var pair in ScpSpawner.SelectedSpawnChances)
                {
                    num7 += pair.Value;

                    ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"num7={num7} pair={pair.Key.nicknameSync.MyNick} / {pair.Value}");

                    if (num7 >= num6)
                    {
                        ApiLoader.Debug($"RoleGeneration.GenerateScpList", $"Chance success (num7={num7} num6={num6})");

                        candidates.RemoveAll(p => p.Hub == pair.Key);
                        players.RemoveAll(p => p.Hub == pair.Key);

                        roles[ExPlayer.Get(pair.Key)] = scpRole;

                        ApiLoader.Debug($"RoleGeneration.ChooseScps", $"Chosen SCP {pair.Key.nicknameSync.MyNick} as {scpRole}");
                    }
                }
            }
            #endregion
        }
    }
}