using Footprinting;

using GameCore;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Attributes;
using LabExtended.API.Enums;
using LabExtended.Commands.Attributes;

using LightContainmentZoneDecontamination;

using NorthwoodLib.Pools;

using PlayerRoles;
using PlayerRoles.RoleAssign;

using RoundRestarting;

using UnityEngine;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
#pragma warning disable CS8629 // Nullable value type may be null.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.API
{
    /// <summary>
    /// A class used for managing the round.
    /// </summary>
    [CommandPropertyAlias("round")]
    public static class ExRound
    {
        /// <summary>
        /// Represents the status of a lock.
        /// </summary>
        public class LockStatus
        {
            private bool isRound;
            
            internal LockStatus(bool isRound)
                => this.isRound = isRound;

            /// <summary>
            /// Whether or not this lock is active.
            /// </summary>
            public bool IsActive
            {
                get => field || (isRound ? RoundSummary.RoundLock : RoundStart.LobbyLock);
                internal set => field = value;
            }
            
            /// <summary>
            /// Gets the time at which this lock was enabled.
            /// </summary>
            public DateTime Time { get; internal set; }
            
            /// <summary>
            /// Gets the footprint of the player who enabled the lock.
            /// </summary>
            public Footprint? Footprint { get; internal set; }
            
            /// <summary>
            /// Gets the player who enabled the lock.
            /// </summary>
            public ExPlayer? Player { get; internal set; }

            /// <summary>
            /// Enables the lock.
            /// </summary>
            /// <param name="enabledBy">The player who enabled the lock.</param>
            /// <returns>true if the lock was enabled</returns>
            public bool Enable(ExPlayer? enabledBy = null)
            {
                if (IsActive)
                    return false;
                
                IsActive = true;
                
                Time = DateTime.Now;
                Player = enabledBy ?? ExPlayer.Host;
                
                Footprint = Player.Footprint;

                if (isRound)
                    RoundSummary.RoundLock = true;
                else
                    RoundStart.LobbyLock = true;
                
                if (!Player.IsHost)
                    ApiLog.Info("LabExtended", $"Player &3{Player.Nickname}&r (&6{Player.UserId}&r) has &2ENABLED&r " +
                                                   $"&1{(isRound ? "Round Lock" : "Lobby Lock")}&r");
                
                return true;
            }

            /// <summary>
            /// Disables the lock.
            /// </summary>
            /// <param name="disabledBy">The player who disabled the round lock.</param>
            /// <returns>true if the lock was disabled</returns>
            public bool Disable(ExPlayer? disabledBy = null)
            {
                if (!IsActive)
                    return false;
                
                IsActive = false;
                
                Time = DateTime.Now;
                
                Player = null;
                Footprint = null;

                if (isRound)
                    RoundSummary.RoundLock = false;
                else
                    RoundStart.LobbyLock = false;
                
                if (disabledBy != null && !disabledBy.IsHost)
                    ApiLog.Info("LabExtended", $"Player &3{disabledBy.Nickname}&r (&6{disabledBy.UserId}&r) has &1DISABLED&r " +
                                                   $"&1{(isRound ? "Round Lock" : "Lobby Lock")}&r");
                
                return true;
            }
        }
        
        private static Vector3? _roundStartPosition;
        private static GameObject? _roundStart;
        
        /// <summary>
        /// Gets the round's number.
        /// </summary>
        [CommandPropertyAlias("number")]
        public static short RoundNumber { get; internal set; }

        /// <summary>
        /// Gets the round's current state.
        /// </summary>
        [CommandPropertyAlias("state")]
        public static RoundState State { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the round has ended.
        /// </summary>
        [CommandPropertyAlias("isEnded")]
        public static bool IsEnded => State is RoundState.Ended;
        
        /// <summary>
        /// Gets a value indicating whether or not the round is currently ending.
        /// </summary>
        [CommandPropertyAlias("isEnding")]
        public static bool IsEnding => State is RoundState.Ending;

        /// <summary>
        /// Gets a value indicating whether the round is in progress.
        /// </summary>
        [CommandPropertyAlias("isRunning")]
        public static bool IsRunning => State is RoundState.InProgress;

        /// <summary>
        /// Gets a value indicating whether the round is waiting for players.
        /// </summary>
        [CommandPropertyAlias("isWaitingForPlayers")]
        public static bool IsWaitingForPlayers => State is RoundState.WaitingForPlayers;

        /// <summary>
        /// Gets a value indicating whether or not SCP-079 was recontained.
        /// </summary>
        [CommandPropertyAlias("isScp079Recontained")]
        public static bool IsScp079Recontained { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether or not Light Containmetn Zone has started decontaminating.
        /// </summary>
        [CommandPropertyAlias("isLczDecontaminated")]
        public static bool IsLczDecontaminated => DecontaminationController.Singleton?.IsDecontaminating ?? false;

        /// <summary>
        /// Gets the <see cref="GameObject"/> of the background of the waiting for players screen.
        /// </summary>
        public static GameObject? RoundStartBackground => _roundStart ??= GameObject.Find("StartRound");

        /// <summary>
        /// Gets the lock controller for round lock.
        /// </summary>
        public static LockStatus RoundLock { get; } = new(true);

        /// <summary>
        /// Gets the lock controller for lobby lock.
        /// </summary>
        public static LockStatus LobbyLock { get; } = new(false);

        /// <summary>
        /// Gets the date of this round's start.
        /// </summary>
        [CommandPropertyAlias("startTime")]
        public static DateTime StartedAt { get; internal set; }
        
        /// <summary>
        /// Gets the amount of time that has passed since the round started or <see cref="TimeSpan.Zero"/> if the round isn't running.
        /// </summary>
        [CommandPropertyAlias("duration")]
        public static TimeSpan Duration => IsRunning ? DateTime.Now - StartedAt : TimeSpan.Zero;

        /// <summary>
        /// Gets or sets the forced decontamination status.
        /// </summary>
        [CommandPropertyAlias("decontaminationStatus")]
        public static DecontaminationController.DecontaminationStatus DecontaminationStatus
        {
            get => DecontaminationController.Singleton?.NetworkDecontaminationOverride ?? DecontaminationController.DecontaminationStatus.None;
            set => DecontaminationController.Singleton!.NetworkDecontaminationOverride = value;
        }

        /// <summary>
        /// Gets the decontamination phase.
        /// </summary>
        [CommandPropertyAlias("decontaminationPhase")]
        public static DecontaminationController.DecontaminationPhase.PhaseFunction DecontaminationPhase
        {
            get
            {
                if (DecontaminationController.Singleton is null)
                    return DecontaminationController.DecontaminationPhase.PhaseFunction.None;
                
                if (DecontaminationController.Singleton._nextPhase < 0 
                    || DecontaminationController.Singleton._nextPhase >= DecontaminationController.Singleton.DecontaminationPhases.Length)
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
        [CommandPropertyAlias("isLocked")]
        public static bool IsRoundLocked
        {
            get => RoundLock.IsActive;
            set
            {
                if (value == RoundLock.IsActive)
                    return;

                if (value)
                    RoundLock.Enable();
                else
                    RoundLock.Disable();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the round can start.
        /// </summary>
        [CommandPropertyAlias("isLobbyLocked")]
        public static bool IsLobbyLocked
        {
            get => LobbyLock.IsActive;
            set
            {
                if (value == LobbyLock.IsActive)
                    return;

                if (value)
                    LobbyLock.Enable();
                else
                    LobbyLock.Disable();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should restart once the round ends.
        /// </summary>
        public static bool ShouldRestartOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Restart;
            set => RoundEndAction = value 
                ? ServerStatic.NextRoundAction.Restart 
                : (RoundEndAction is ServerStatic.NextRoundAction.Restart 
                    ? ServerStatic.NextRoundAction.DoNothing 
                    : ServerStatic.NextRoundAction.Shutdown);
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the server should shut down once the round ends.
        /// </summary>
        public static bool ShouldShutdownOnRoundEnd
        {
            get => RoundEndAction is ServerStatic.NextRoundAction.Shutdown;
            set => RoundEndAction = value 
                ? ServerStatic.NextRoundAction.Shutdown 
                : (RoundEndAction is ServerStatic.NextRoundAction.Shutdown 
                    ? ServerStatic.NextRoundAction.DoNothing 
                    : ServerStatic.NextRoundAction.Restart);
        }

        /// <summary>
        /// Gets or sets the current extra target count for SCPs.
        /// </summary>
        [CommandPropertyAlias("extraTargets")]
        public static int ExtraTargetCount
        {
            get => RoundSummary.singleton?.Network_extraTargets ?? 0;
            set => RoundSummary.singleton!.Network_extraTargets = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped Class-Ds.
        /// </summary>
        [CommandPropertyAlias("escapedClassD")]
        public static int EscapedClassDs
        {
            get => RoundSummary.EscapedClassD;
            set => RoundSummary.EscapedClassD = value;
        }

        /// <summary>
        /// Gets or sets the amount of escaped scientists.
        /// </summary>
        [CommandPropertyAlias("escapedScientists")]
        public static int EscapedScientists
        {
            get => RoundSummary.EscapedScientists;
            set => RoundSummary.EscapedScientists = value;
        }

        /// <summary>
        /// Gets or sets the amount of total kills.
        /// </summary>
        [CommandPropertyAlias("kills")]
        public static int Kills
        {
            get => RoundSummary.Kills;
            set => RoundSummary.Kills = value;
        }

        /// <summary>
        /// Gets or sets the amount of SCPs that have survived the round.
        /// </summary>
        [CommandPropertyAlias("scps")]
        public static int SurvivingSCPs
        {
            get => RoundSummary.SurvivingSCPs;
            set => RoundSummary.SurvivingSCPs = value;
        }

        /// <summary>
        /// Gets or sets the amount of kills that the SCPs did.
        /// </summary>
        [CommandPropertyAlias("scpKills")]
        public static int KillsByScp
        {
            get => RoundSummary.KilledBySCPs;
            set => RoundSummary.KilledBySCPs = value;
        }

        /// <summary>
        /// Gets the amount of players that have been changed into zombies.
        /// </summary>
        [CommandPropertyAlias("zombies")]
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
        /// Hides the round ending screen.
        /// </summary>
        public static void CancelRoundEnd()
            => RoundSummary.singleton?.CancelRoundEnding();

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
            void DecideHumans(List<ExPlayer?> players, Dictionary<ExPlayer?, RoleTypeId> roles, Team[] queue, int size)
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

            void DecideScps(List<ExPlayer?> players, Dictionary<ExPlayer?, RoleTypeId> roles, int scps)
            {
                List<ExPlayer?>? candidates = ListPool<ExPlayer>.Shared.Rent();

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
                        if (!RoleAssigner.CheckPlayer(player.ReferenceHub))
                            continue;

                        var tickets = ticketLoader.GetTickets(player.ReferenceHub, 10);

                        ticketLoader.ModifyTickets(player.ReferenceHub, tickets + 2);
                    }

                    foreach (var player in candidates)
                        ticketLoader.ModifyTickets(player.ReferenceHub, 10);
                }

                while (scpQueue.TryDequeue(out var scpRole) && ExServer.IsRunning)
                    ChooseScps(candidates, players, roles, scpRole, scpQueue);

                ListPool<ExPlayer>.Shared.Return(candidates);
            }

            void GenerateScpList(List<ExPlayer?> candidates, Dictionary<ExPlayer?, RoleTypeId> roles, List<ExPlayer?> players, int scps)
            {
                if (scps <= 0)
                    return;

                using (var ticketLoader = new ScpTicketsLoader())
                {
                    var num = 0;

                    foreach (var player in players)
                    {
                        var tickets = ticketLoader.GetTickets(player.ReferenceHub, 10);

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
                            var tickets = ticketLoader.GetTickets(player.ReferenceHub, 10);

                            for (int i = 0; i < scps; i++)
                                num3 *= tickets;

                            potential.Add(new ScpPlayerPicker.PotentialScp { Player = player.ReferenceHub, Weight = num3 });
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

            void ChooseScps(List<ExPlayer?> candidates, List<ExPlayer?> players, Dictionary<ExPlayer?, RoleTypeId> roles, RoleTypeId scpRole, Queue<RoleTypeId> scpQueue)
            {
                ScpSpawner.ChancesBuffer.Clear();

                var num = 1;
                var num2 = 0;

                foreach (var player in candidates)
                {
                    var num3 = ScpSpawner.GetPreferenceOfPlayer(player.ReferenceHub, scpRole);

                    foreach (var otherScp in scpQueue)
                        num3 -= ScpSpawner.GetPreferenceOfPlayer(player.ReferenceHub, otherScp);

                    num2++;

                    ScpSpawner.ChancesBuffer[player.ReferenceHub] = num3;
                    
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
                        candidates.RemoveAll(p => p.ReferenceHub == pair.Key);
                        players.RemoveAll(p => p.ReferenceHub == pair.Key);

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
        private static void OnInit()
        {
            InternalEvents.OnRoundWaiting += OnRoundWait;
        }
    }
}