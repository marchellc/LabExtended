using Footprinting;

using GameCore;

using LabExtended.API.Enums;
using LabExtended.API.Internal;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Events;
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
        
        private static Vector3? _roundStartPosition;

        private static GameObject _roundStart;
        
        /// <summary>
        /// Gets the round's number.
        /// </summary>
        public static short RoundNumber { get; internal set; }

        /// <summary>
        /// Gets the round's current state.
        /// </summary>
        public static RoundState State { get; internal set; }

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
        /// Gets the <see cref="GameObject"/> of the background of the waiting for players screen.
        /// </summary>
        public static GameObject RoundStartBackground
        {
            get
            {
                if (_roundStart is null)
                    _roundStart = GameObject.Find("StartRound");

                return _roundStart;
            }
        }

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
        public static Footprint LobbyLockEnabledByFootprint => _lobbyLockTracking?.EnabledByFootprint ?? default;

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
                    ApiLog.Debug("Round API", $"Round Lock enabled by &3{RoundLockEnabledBy?.Name ?? "null"}&r (&6{RoundLockEnabledBy?.UserId ?? null}&r)");
                else
                    ApiLog.Debug("Round API", $"Round Lock disabled.");
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
                    ApiLog.Debug("Round API", $"Lobby Lock enabled by &3{LobbyLockEnabledBy?.Name ?? "null"}&r (&6{LobbyLockEnabledBy?.UserId ?? null}&r)");
                else
                    ApiLog.Debug("Round API", $"Lobby Lock disabled.");
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
                if (DecontaminationController.Singleton is null)
                    return DecontaminationController.DecontaminationPhase.PhaseFunction.None;
                
                if (DecontaminationController.Singleton._nextPhase < 0 || DecontaminationController.Singleton._nextPhase >= DecontaminationController.Singleton.DecontaminationPhases.Length)
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
        /// Gets or sets the current extra target count for SCPs.
        /// </summary>
        public static int ExtraTargetCount
        {
            get => RoundSummary.singleton?.Network_extraTargets ?? 0;
            set => RoundSummary.singleton!.Network_extraTargets = value;
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
        /// Hides the background while in "Waiting for players"
        /// </summary>
        public static void HideRoundStartBackground()
        {
            _roundStartPosition ??= RoundStartBackground.transform.position;
            _roundStart.transform.position = Vector3.zero;
        }

        /// <summary>
        /// Shows the background while in "Waiting for players"
        /// <remarks>This method CANNOT be used without first hiding the screen, it will throw a null reference exception!</remarks>
        /// </summary>
        public static void ShowRoundStartBackground()
            => _roundStart.transform.position = _roundStartPosition.Value;

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

                var players = new List<ExPlayer>(ExPlayer.AllPlayers.Where(x => !x.IsServer && x.IsOnline && x.Role.Is(RoleTypeId.None)));
                var scps = 0;

                for (int i = 0; i < players.Count; i++)
                {
                    if (RoleAssigner._totalQueue[i % totalIndex] is Team.SCPs)
                        scps++;
                }

                if (scps > 0)
                {
                    DecideScps(players, roles, scps);

                    players.RemoveAll(roles.ContainsKey);
                }

                if (players.Count > 0)
                    DecideHumans(players, roles, RoleAssigner._humanQueue, humanIndex);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Round API", $"Failed to generate roles!\n{ex.ToColoredString()}");
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

                for (int i = 0; i < players.Count; i++)
                {
                    candidates.Clear();

                    var num = int.MaxValue;
                    var role = array[i];

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
                            }
                        }

                        if (num2 <= num)
                        {
                            if (num2 < num)
                                candidates.Clear();

                            candidates.Add(player);
                            num = num2;
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        var random = candidates.RandomItem();

                        roles[random] = role;
                    }
                }

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void DecideScps(List<ExPlayer> players, Dictionary<ExPlayer, RoleTypeId> roles, int scps)
            {
                var candidates = ListPool<ExPlayer>.Shared.Rent();

                GenerateScpList(candidates, roles, players, scps);

                var scpQueue = new Queue<RoleTypeId>();

                for (int i = 0; i < scps; i++)
                {
                    var nextScp = ScpSpawner.NextScp;

                    if (!scpQueue.Contains(nextScp))
                    {
                        scpQueue.Enqueue(nextScp);
                    }
                }

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

                while (scpQueue.TryDequeue(out var scpRole) && ExServer.IsRunning)
                    ChooseScps(candidates, players, roles, scpRole, scpQueue);

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void GenerateScpList(List<ExPlayer> candidates, Dictionary<ExPlayer, RoleTypeId> roles, List<ExPlayer> players, int scps)
            {
                if (scps <= 0)
                    return;

                using (var ticketLoader = new ScpTicketsLoader())
                {
                    var num = 0;

                    foreach (var player in players)
                    {
                        var tickets = ticketLoader.GetTickets(player.Hub, 10);

                        if (tickets >= num)
                        {
                            if (tickets > num)
                            {
                                candidates.Clear();
                            }

                            num = tickets;
                            candidates.Add(player);
                        }
                    }

                    if (candidates.Count > 1)
                    {
                        candidates.Clear();
                        candidates.Add(candidates.RandomItem());
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
                        }
                    }

                    while (scps > 0 && ExServer.IsRunning)
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
                ScpSpawner.ChancesBuffer.Clear();

                var num = 1;
                var num2 = 0;

                foreach (var player in candidates)
                {
                    var num3 = ScpSpawner.GetPreferenceOfPlayer(player.Hub, scpRole);

                    foreach (var otherScp in scpQueue)
                    {
                        num3 -= ScpSpawner.GetPreferenceOfPlayer(player.Hub, otherScp);
                        ApiLog.Debug($"RoleGeneration.GenerateScpList", $"Preference now {num3}");
                    }

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

                    num4 += num5;
                }

                var num6 = num4 * UnityEngine.Random.value;
                var num7 = 0f;

                foreach (var pair in ScpSpawner.SelectedSpawnChances)
                {
                    num7 += pair.Value;

                    if (num7 >= num6)
                    {
                        candidates.RemoveAll(p => p.Hub == pair.Key);
                        players.RemoveAll(p => p.Hub == pair.Key);

                        roles[ExPlayer.Get(pair.Key)] = scpRole;
                    }
                }
            }
            #endregion
        }

        private static void OnRoundWait()
        {
            _roundStart = null;

            IsScp079Recontained = false;
            
            StartedAt = DateTime.MinValue;
            State = RoundState.WaitingForPlayers;

            RoundNumber++;
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            InternalEvents.OnRoundWaiting += OnRoundWait;
        }
    }
}