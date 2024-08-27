using Interactables;

using LabExtended.API.Modules;

using LabExtended.Core;
using LabExtended.Core.Ticking.Distributors.Unity;
using LabExtended.Core.Ticking.Interfaces;
using LabExtended.Core.Ticking.Timers;

using Mirror;

using UnityEngine;
using UnityEngine.AI;

namespace LabExtended.API.Npcs.Navigation
{
    /// <summary>
    /// Module used for NPC navigation.
    /// </summary>
    public class NavigationModule : Module
    {
        public static LayerMask InteractionMask { get; } = new LayerMask() { value = 134374145 };

        /// <summary>
        /// Gets the module's <see cref="NavMeshAgent"/>.
        /// </summary>
        public NavMeshAgent NavAgent { get; internal set; }

        /// <summary>
        /// Gets the NPC.
        /// </summary>
        public NpcHandler Npc { get; internal set; }

        /// <inheritdoc/>
        public override ITickTimer TickTimer { get; } = new StaticTickTimer(500);

        /// <inheritdoc/>
        public override Type TickType { get; } = typeof(UnityTickDistributor);

        /// <summary>
        /// Whether or not to allow the NPC to interact.
        /// </summary>
        public bool AllowInteractions { get; set; } = true;

        /// <summary>
        /// Whether or not this module was initialized yet.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets the NPC's target position. This overrides <see cref="PlayerTarget"/>.
        /// </summary>
        public Vector3? TargetPosition { get; set; }

        /// <summary>
        /// Gets or sets the player target to follow.
        /// </summary>
        public ExPlayer PlayerTarget { get; set; }

        /// <inheritdoc/>
        public override void OnStarted()
        {
            base.OnStarted();
            NavigationMesh.Prepare();
        }

        /// <inheritdoc/>
        public override void OnTick()
        {
            base.OnTick();

            if (NavAgent is null)
                return;

            if (Npc is null)
                return;

            if (!TargetPosition.HasValue && PlayerTarget is null)
            {
                if (!NavAgent.isStopped)
                    NavAgent.isStopped = true;

                return;
            }
            else
            {
                NavAgent.destination = TargetPosition.HasValue ? TargetPosition.Value : PlayerTarget.Position.Position;

                if (!NavAgent.isOnNavMesh)
                {
                    NavAgent.enabled = false;
                    NavAgent.enabled = true;

                    return;
                }

                if (!Npc.Player.Role.IsAlive)
                {
                    if (!NavAgent.isStopped)
                        NavAgent.isStopped = true;

                    return;
                }
                else if (NavAgent.isStopped)
                    NavAgent.isStopped = false;

                if (!AllowInteractions)
                    return;

                if (Physics.Raycast(new Ray(Npc.Player.CameraTransform.position, Npc.Player.CameraTransform.forward), out var hit, 300f, InteractionMask))
                {
                    if (!hit.collider.TryGetComponent<InteractableCollider>(out var interactableCollider))
                        return;

                    if (interactableCollider.Target is null || interactableCollider.Target is not IInteractable interactable)
                        return;

                    if (!InteractionCoordinator.GetSafeRule(interactable).ClientCanInteract(interactableCollider, hit))
                        return;

                    if (interactableCollider.Target is not NetworkBehaviour networkBehaviour)
                        return;

                    Npc.Hub.interCoordinator.UserCode_CmdServerInteract__NetworkIdentity__Byte(networkBehaviour.netIdentity, interactableCollider.ColliderId);
                }
            }
        }

        /// <inheritdoc/>
        public override void OnStopped()
        {
            base.OnStopped();

            if (NavAgent != null)
            {
                UnityEngine.Object.Destroy(NavAgent);
                NavAgent = null;
            }
        }

        internal void Initialize(NpcHandler npcHandler)
        {
            try
            {
                if (Npc != null)
                    return;

                Npc = npcHandler;

                NavAgent = npcHandler.Hub.gameObject.AddComponent<NavMeshAgent>();
                NavAgent.baseOffset = 0.98f;
                NavAgent.updateRotation = true;
                NavAgent.angularSpeed = 360;
                NavAgent.acceleration = 600;
                NavAgent.radius = 0.1f;
                NavAgent.areaMask = 1;
                NavAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Navigation API", ex);
            }
        }
    }
}