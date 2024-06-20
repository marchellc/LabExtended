using Common.IO.Collections;

using Hazards;

using LabExtended.Core;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;
using LabExtended.Interfaces;
using LabExtended.Utilities;

using MapGeneration;

using MEC;

using Mirror;

using PlayerRoles;
using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.API.Map
{
    /// <summary>
    /// A wrapper for the <see cref="TeslaGate"/> class.
    /// </summary>
    public class ExTeslaGate :
        Wrapper<TeslaGate>,

        IMapObject,
        IDamageObject
    {
        static ExTeslaGate() => Timing.RunCoroutine(UpdateGates());

        internal static readonly LockedDictionary<TeslaGate, ExTeslaGate> _wrappers = new LockedDictionary<TeslaGate, ExTeslaGate>();
        internal static readonly ProfilerMarker _marker = new ProfilerMarker("Tesla Gate Update");

        internal static bool _pauseUpdate;

        internal ExTeslaGate(TeslaGate baseValue) : base(baseValue) { }

        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> of roles ignored by all tesla gates.
        /// </summary>
        public static HashSet<RoleTypeId> IgnoredRoles { get; } = new HashSet<RoleTypeId>();

        /// <summary>
        /// Gets a <see cref="HashSet{T}"/> of teams ignored by all tesla gates.
        /// </summary>
        public static HashSet<Team> IgnoredTeams { get; } = new HashSet<Team>();

        /// <summary>
        /// Gets or sets a custom tick rate for tesla gates.
        /// </summary>
        public static int TickRate { get; set; } = -1;

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
        public ExPlayer ClosestPlayer => ExPlayer.Players.Where(p => p.Role.IsAlive).OrderBy(p => p.DistanceTo(Position)).FirstOrDefault();

        /// <summary>
        /// Gets the closest SCP player.
        /// </summary>
        public ExPlayer ClosestScp => ExPlayer.Players.Where(p => p.Role.IsScp).OrderBy(p => p.DistanceTo(Position)).FirstOrDefault();

        /// <summary>
        /// Gets the tesla's position.
        /// </summary>
        public Vector3 Position => Transform.position;

        /// <summary>
        /// Gets the tesla's rotation.
        /// </summary>
        public Quaternion Rotation => Quaternion.Euler(Base.localRotation);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Player"/> which contains all the players inside the hurt range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInHurtRange => ExPlayer.Players.Where(IsInHurtRange);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Player"/> which contains all the players inside the idle range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInIdleRange => ExPlayer.Players.Where(IsInIdleRange);

        /// <summary>
        /// Gets a <see cref="IEnumerable{T}"/> of <see cref="Player"/> which contains all the players inside the trigger range.
        /// </summary>
        public IEnumerable<ExPlayer> PlayersInTriggerRange => ExPlayer.Players.Where(IsInTriggerRange);

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
            => player != null && player.Role.IsAlive && player.DistanceTo(Position) <= TriggerRange * 2.2f;

        /// <summary>
        /// Gets a value indicating whether or not a specific player is in idle range.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if the player is in idle range, otherwise <see langword="false"/>.</returns>
        public bool IsInIdleRange(ExPlayer player)
            => player != null && player.Role.IsAlive && Base.IsInIdleRange(player.Hub);

        /// <summary>
        /// Gets a value indicating whether or not a specific player is in trigger range.
        /// </summary>
        /// <param name="player">The player to check.</param>
        /// <returns><see langword="true"/> if the player is in trigger range, otherwise <see langword="false"/>.</returns>
        public bool IsInTriggerRange(ExPlayer player)
            => player != null && player.Role.IsAlive && Base.PlayerInRange(player.Hub);

        /// <inheritdoc/>
        public void Damage(ExPlayer player, float amount)
            => player.Hub.playerStats.DealDamage(new UniversalDamageHandler(amount, DeathTranslations.Tesla));

        /// <inheritdoc/>
        public void Kill(ExPlayer player)
            => player.Hub.playerStats.DealDamage(new UniversalDamageHandler(-1f, DeathTranslations.Tesla));

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
            Timing.RunCoroutine(Base.ServerSideWaitForAnimation());
            Base.RpcPlayAnimation();
        }

        /// <summary>
        /// Deletes this tesla gate.
        /// </summary>
        public void Delete()
        {
            _wrappers.Remove(Base);
            NetworkServer.Destroy(GameObject);
        }

        internal void InternalUpdate() { }

        private static IEnumerator<float> UpdateGates()
        {
            while (true)
            {
                if (TickRate > 0)
                    yield return Timing.WaitForSeconds(TickRate * 1000f);
                else
                    yield return Timing.WaitForOneFrame;

                _marker.MarkStart(_wrappers.Count.ToString());

                if (!_pauseUpdate)
                {
                    foreach (var pair in _wrappers)
                    {
                        try
                        {
                            if (pair.Value.IsDisabled || pair.Value.GameObject is null)
                                continue;

                            pair.Value.InternalUpdate();
                        }
                        catch (Exception ex)
                        {
                            ExLoader.Error("Extended API", $"Failed to update tesla gates!\n{ex.ToColoredString()}");
                        }
                    }
                }

                _marker.MarkEnd();
            }
        }
    }
}