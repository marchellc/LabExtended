using Hazards;

using LabExtended.API.Interfaces;
using LabExtended.API.Wrappers;

using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Events;
using LabExtended.Events.Map;
using LabExtended.Events.Player;

using LabExtended.Extensions;

using MapGeneration;

using PlayerRoles;
using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.API
{
    /// <summary>
    /// A wrapper for the <see cref="ExTeslaGate"/> class.
    /// </summary>
    public class ExTeslaGate :
        NetworkWrapper<TeslaGate>,

        IDamageObject
    {
        /// <summary>
        /// Gets a lookup table for each tesla gate.
        /// </summary>
        public static Dictionary<TeslaGate, ExTeslaGate> Lookup { get; } = new();

        /// <summary>
        /// Gets a list of all tesla gates.
        /// </summary>
        public static List<ExTeslaGate> Gates { get; } = new();

        /// <summary>
        /// Tries to get an initialized wrapper.
        /// </summary>
        /// <param name="gate">The base object to find the wrapper variant of.</param>
        /// <param name="wrapper">The found wrapper instance.</param>
        /// <returns>true if the wrapper was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(TeslaGate gate, out ExTeslaGate wrapper)
        {
            if (gate is null)
                throw new ArgumentNullException(nameof(gate));
            
            return Lookup.TryGetValue(gate, out wrapper);
        }

        /// <summary>
        /// Gets an initialized wrapper.
        /// </summary>
        /// <param name="gate">The base object to find the wrapper variant of.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static ExTeslaGate Get(TeslaGate gate)
        {
            if (gate is null)
                throw new ArgumentNullException(nameof(gate));

            if (!Lookup.TryGetValue(gate, out var wrapper))
                throw new KeyNotFoundException($"Could not find a Tesla Gate (ID={gate.netId})");
            
            return wrapper;
        }
        
        internal ExTeslaGate(TeslaGate baseValue) : base(baseValue) { }

        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> of roles ignored by all tesla gates.
        /// </summary>
        public static HashSet<RoleTypeId> IgnoredRoles { get; } = new();

        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> of teams ignored by all tesla gates.
        /// </summary>
        public static HashSet<Team> IgnoredTeams { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether or not this tesla gate can be triggered.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <inheritdoc/>
        public bool IsDamageDisabled { get; set; }

        /// <summary>
        /// Gets the tesla's <see cref="UnityEngine.GameObject"/>.
        /// </summary>
        public GameObject GameObject => Base.gameObject;

        /// <summary>
        /// Gets the tesla's <see cref="UnityEngine.Transform"/>.
        /// </summary>
        public Transform Transform => Base.transform;

        /// <summary>
        /// Gets the tesla's current room.
        /// </summary>
        public RoomIdentifier Room => Base.Room;

        /// <summary>
        /// Gets the closest player.
        /// </summary>
        public ExPlayer ClosestPlayer => ExPlayer.Players.Where(p => p.Role.IsAlive).OrderBy(p => p.Position.DistanceTo(Position)).FirstOrDefault();

        /// <summary>
        /// Gets the closest SCP player.
        /// </summary>
        public ExPlayer ClosestScp => ExPlayer.Players.Where(p => p.Role.IsScp).OrderBy(p => p.Position.DistanceTo(Position)).FirstOrDefault();

        /// <summary>
        /// Gets the tesla's rotation.
        /// </summary>
        public new Quaternion Rotation => Quaternion.Euler(Base.localRotation);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="ExPlayer"/> which contains all the players inside the hurt range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInHurtRange
        {
            get
            {
                for (var i = 0; i < ExPlayer.Players.Count; i++)
                {
                    var player = ExPlayer.Players[i];
                    
                    if (!player || !IsInHurtRange(player))
                        continue;

                    yield return player;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="ExPlayer"/> which contains all the players inside the idle range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInIdleRange 
        {
            get
            {
                for (var i = 0; i < ExPlayer.Players.Count; i++)
                {
                    var player = ExPlayer.Players[i];
                    
                    if (!player || !IsInIdleRange(player))
                        continue;

                    yield return player;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="ExPlayer"/> which contains all the players inside the trigger range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInTriggerRange
        {
            get
            {
                for (var i = 0; i < ExPlayer.Players.Count; i++)
                {
                    var player = ExPlayer.Players[i];
                    
                    if (!player || !IsInTriggerRange(player))
                        continue;

                    yield return player;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not a shock is in progress.
        /// </summary>
        public bool IsShocking => Base.InProgress;

        /// <summary>
        /// Gets or sets the tesla's hurt range.
        /// </summary>
        public Vector3 HurtRange
        {
            get => Base.sizeOfKiller;
            set => Base.sizeOfKiller = value;
        }

        /// <summary>
        /// Gets or sets the tesla's inactive time.
        /// </summary>
        public float InactiveTime
        {
            get => Base.NetworkInactiveTime;
            set => Base.NetworkInactiveTime = value;
        }

        /// <summary>
        /// Gets or sets the tesla gate's windup time to wait before generating the shock.
        /// </summary>
        public float ActivationTime
        {
            get => Base.windupTime;
            set => Base.windupTime = value;
        }

        /// <summary>
        /// Gets or sets the tesla gate's cooldown to wait before the next shock.
        /// </summary>
        public float CooldownTime
        {
            get => Base.cooldownTime;
            set => Base.cooldownTime = value;
        }

        /// <summary>
        /// Gets or sets the tesla gate's distance from which can be triggered.
        /// </summary>
        public float TriggerRange
        {
            get => Base.sizeOfTrigger;
            set => Base.sizeOfTrigger = value;
        }

        /// <summary>
        /// Gets or sets the tesla gate's distance from which players must stand for it to enter idle mode.
        /// </summary>
        public float IdleRange
        {
            get => Base.distanceToIdle;
            set => Base.distanceToIdle = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tesla gate is idling.
        /// </summary>
        public bool IsIdling
        {
            get => Base.isIdling;
            set
            {
                if (value)
                    Base.RpcDoIdle();
                else
                    Base.RpcDoneIdling();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the tesla gate's next burst should be treated as instant burst.
        /// </summary>
        public bool UseInstantBurst
        {
            get => Base.next079burst;
            set => Base.next079burst = value;
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="TantrumEnvironmentalHazard"/> which contains all the tantrums to destroy.
        /// </summary>
        public List<TantrumEnvironmentalHazard> TantrumsToDestroy
        {
            get => Base.TantrumsToBeDestroyed;
            set => Base.TantrumsToBeDestroyed = value;
        }

        /// <summary>
        /// Gets a value indicating whether or not a specific player is in hurt range.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if the player is in hurt range, otherwise <see langword="false"/>.</returns>
        public bool IsInHurtRange(ExPlayer player)
            => player != null && player.Role.IsAlive && player.Position.DistanceTo(Position) <= TriggerRange * 2.2f;

        /// <summary>
        /// Gets a value indicating whether or not a specific player is in idle range.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if the player is in idle range, otherwise <see langword="false"/>.</returns>
        public bool IsInIdleRange(ExPlayer player)
            => player != null && player.Role.IsAlive && Base.IsInIdleRange(player.ReferenceHub);

        /// <summary>
        /// Gets a value indicating whether or not a specific player is in trigger range.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if the player is in trigger range, otherwise <see langword="false"/>.</returns>
        public bool IsInTriggerRange(ExPlayer player)
            => player != null && player.Role.IsAlive && Base.PlayerInRange(player.ReferenceHub);

        /// <inheritdoc/>
        public void Damage(ExPlayer player, float amount)
            => player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(amount, DeathTranslations.Tesla));

        /// <inheritdoc/>
        public void Kill(ExPlayer player)
            => player.ReferenceHub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Tesla));

        /// <inheritdoc/>
        public DamageHandlerBase GetDamageHandler(float damageAmount)
            => new UniversalDamageHandler(damageAmount, DeathTranslations.Tesla);

        /// <summary>
        /// Triggers the tesla gate.
        /// </summary>
        /// <param name="isInstant">A value indicating whether the shock should be treated as instant burst.</param>
        public void Trigger(bool isInstant = false)
        {
            if (isInstant)
                Base.RpcInstantBurst();
            else
                Base.ServerSideCode();
        }

        /// <summary>
        /// Force triggers the tesla gate ignoring the delay between each burst.
        /// </summary>
        public void ForceTrigger()
        {
            MEC.Timing.RunCoroutine(Base.ServerSideWaitForAnimation());
            Base.RpcPlayAnimation();
        }

        private void Update()
        {
            if (IsDisabled || GameObject is null || !Base.isActiveAndEnabled)
                return;

            if (Base.InactiveTime > 0f)
            {
                Base.NetworkInactiveTime = Mathf.Max(0f, Base.InactiveTime - Time.fixedDeltaTime);
                return;
            }

            var shouldIdle = false;
            var shouldTrigger = false;

            foreach (var player in ExPlayer.Players)
            {
                if (!player.Toggles.CanTriggerTesla) continue;
                if (!player.Role.IsAlive) continue;
                
                if (IgnoredRoles.Contains(player.Role.Type) || IgnoredTeams.Contains(player.Role.Team)) 
                    continue;

                if (!shouldIdle)
                    shouldIdle = Base.IsInIdleRange(player.ReferenceHub);

                if (shouldTrigger || !Base.PlayerInRange(player.ReferenceHub) || Base.InProgress) 
                    continue;
                
                if (!HookRunner.RunEvent(new PlayerTriggeringTeslaGateArgs(player, this), true))
                    continue;

                shouldTrigger = true;
            }

            if (shouldTrigger)
            {
                if (!Base.InProgress)
                {
                    var triggerEv = new TeslaGateTriggeringEventArgs(this, Base.next079burst);

                    if (ExMapEvents.OnTeslaGateTriggering(triggerEv))
                    {
                        Base.next079burst = triggerEv.IsInstant;
                        Base.RpcPlayAnimation();

                        MEC.Timing.RunCoroutine(Base.ServerSideWaitForAnimation());
                    }
                }
            }

            if (shouldIdle == Base.isIdling) 
                return;
            
            if (shouldIdle)
            {
                Base.RpcDoIdle();
                ExMapEvents.OnTeslaGateStartedIdling(new(this));
            }
            else
            {
                Base.RpcDoneIdling();
                ExMapEvents.OnTeslaGateStoppedIdling(new(this));
            }
        }

        private static void OnUpdate()
        {
            if (!StaticUnityMethods.IsPlaying)
                return;
            
            Gates.ForEach(gate =>
            {
                try
                {
                    gate.Update();
                }
                catch (Exception ex)
                {
                    ApiLog.Error("LabExtended", $"Failed to update tesla gate {gate.NetId}!\n{ex.ToColoredString()}");
                }
            });
        }

        private static void OnTeslaSpawned(TeslaGate gate)
        {
            if (Lookup.ContainsKey(gate))
                return;

            var wrapper = new ExTeslaGate(gate);

            Gates.Add(wrapper);
            Lookup.Add(gate, wrapper);
        }

        private static void OnTeslaDestroyed(TeslaGate gate)
        {
            if (!Lookup.TryGetValue(gate, out var wrapper))
                return;
            
            Gates.Remove(wrapper);
            Lookup.Remove(gate);
        }

        [LoaderInitialize(1)]
        private static void Init()
        {
            TeslaGate.OnAdded += OnTeslaSpawned;
            TeslaGate.OnRemoved += OnTeslaDestroyed;
            
            StaticUnityMethods.OnFixedUpdate += OnUpdate;

            InternalEvents.OnRoundRestart += Lookup.Clear;
            InternalEvents.OnRoundRestart += Gates.Clear;
        }
    }
}