using Hazards;

using Interactables;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API.Prefabs;
using LabExtended.Attributes;
using LabExtended.Commands.Attributes;
using LabExtended.Core;
using LabExtended.Core.Networking;

using LabExtended.Events;
using LabExtended.Extensions;

using LightContainmentZoneDecontamination;

using MapGeneration;
using MapGeneration.Distributors;

using Mirror;

using PlayerRoles;
using PlayerRoles.Ragdolls;
using PlayerRoles.PlayableScps.Scp3114;

using PlayerStatsSystem;

using RelativePositioning;

using UnityEngine;

using Utils;
using Utils.Networking;

namespace LabExtended.API
{
    /// <summary>
    /// Map management functions.
    /// </summary>
    [CommandPropertyAlias("map")]
    public static class ExMap
    {
        /// <summary>
        /// List of spawned pickups.
        /// </summary>
        [CommandPropertyAlias("pickups")]
        public static List<ItemPickupBase> Pickups { get; } = new();
        
        /// <summary>
        /// List of spawned ragdolls.
        /// </summary>
        [CommandPropertyAlias("ragdolls")]
        public static List<BasicRagdoll> Ragdolls { get; } = new();
        
        /// <summary>
        /// List of spawned lockers.
        /// </summary>
        [CommandPropertyAlias("lockers")]
        public static List<Locker> Lockers { get; } = new();

        /// <summary>
        /// List of locker chambers.
        /// </summary>
        [CommandPropertyAlias("chambers")]
        public static List<LockerChamber> Chambers { get; } = new();

        /// <summary>
        /// List of spawned waypoints.
        /// </summary>
        public static IEnumerable<WaypointBase> Waypoints => WaypointBase.AllWaypoints.Where(x => x != null);
        
        /// <summary>
        /// List of spawned NetID waypoints.
        /// </summary>
        public static IEnumerable<NetIdWaypoint> NetIdWaypoints => NetIdWaypoint.AllNetWaypoints.Where(x => x != null);

        /// <summary>
        /// Amount of ambient clips.
        /// </summary>
        public static int AmbientClipsCount => AmbientSoundPlayer?.clips.Length ?? 0;

        /// <summary>
        /// Gets the AmbientSoundPlayer component.
        /// </summary>
        public static AmbientSoundPlayer? AmbientSoundPlayer { get; private set; }

        /// <summary>
        /// Gets the default color of a room's light.
        /// </summary>
        [CommandPropertyAlias("defaultLightColor")]
        public static Color DefaultLightColor { get; } = Color.clear;

        /// <summary>
        /// Gets or sets the map's seed.
        /// </summary>
        [CommandPropertyAlias("seed")]
        public static int Seed
        {
            get => SeedSynchronizer.Seed;
            set
            {
                if (SeedSynchronizer.MapGenerated)
                    return;

                SeedSynchronizer.Seed = value;
            }
        }

        /// <summary>
        /// Broadcasts a message to all players.
        /// </summary>
        /// <param name="msg">The message to show.</param>
        /// <param name="duration">Duration of the message.</param>
        /// <param name="clearPrevious">Whether or not to clear previous broadcasts.</param>
        /// <param name="flags">Flags of the broadcast.</param>
        public static void Broadcast(object msg, ushort duration, bool clearPrevious = true, Broadcast.BroadcastFlags flags = global::Broadcast.BroadcastFlags.Normal)
        {
            if (clearPrevious)
                global::Broadcast.Singleton?.RpcClearElements();

            global::Broadcast.Singleton?.RpcAddElement(msg.ToString(), duration, flags);
        }

        /// <summary>
        /// Shows a hint to all players.
        /// </summary>
        /// <param name="msg">The message to show.</param>
        /// <param name="duration">Duration of the message.</param>
        /// <param name="isPriority">Whether or not to show this message immediately.</param>
        public static void ShowHint(object msg, ushort duration, bool isPriority = false)
            => ExPlayer.Players.ForEach(x => x.SendHint(msg, duration, isPriority));

        /// <summary>
        /// Starts Light Containment Zone decontamination.
        /// </summary>
        public static void StartDecontamination()
            => DecontaminationController.Singleton?.ForceDecontamination();

        /// <summary>
        /// Plays a random ambient clip.
        /// </summary>
        public static void PlayAmbientSound()
            => AmbientSoundPlayer?.GenerateRandom();

        /// <summary>
        /// Plays the specified ambient clip.
        /// </summary>
        /// <param name="id">Index of the clip to play.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void PlayAmbientSound(int id)
        {
            if (id < 0 || id >= AmbientClipsCount)
                throw new ArgumentOutOfRangeException(nameof(id));

            AmbientSoundPlayer?.RpcPlaySound(AmbientSoundPlayer.clips[id].index);
        }

        /// <summary>
        /// Spawns a new tantrum.
        /// </summary>
        /// <param name="position">Where to spawn the tantrum.</param>
        /// <param name="setActive">Whether or not to set the tantrum as active.</param>
        /// <returns></returns>
        public static TantrumEnvironmentalHazard PlaceTantrum(Vector3 position, bool setActive = true)
        {
            var instance = PrefabList.Tantrum.Spawn<TantrumEnvironmentalHazard>();

            if (!setActive)
                instance.SynchronizedPosition = new RelativePosition(position);
            else
                instance.SynchronizedPosition = new RelativePosition(position + (Vector3.up * 0.25f));

            instance._destroyed = !setActive;

            NetworkServer.Spawn(instance.gameObject);
            return instance;
        }

        /// <summary>
        /// Spawns a grenade.
        /// </summary>
        /// <param name="position">The position to spawn the grenade at.</param>
        /// <param name="type">The type of grenade.</param>
        /// <param name="attacker">The attacker.</param>
        public static void Explode(Vector3 position, ExplosionType type = ExplosionType.Grenade, ExPlayer? attacker = null)
            => ExplosionUtils.ServerExplode(position, (attacker ?? ExPlayer.Host).Footprint, type);

        /// <summary>
        /// Spawns an explosion effect.
        /// </summary>
        /// <param name="position">Where to spawn the effect.</param>
        /// <param name="type">Type of effect.</param>
        public static void SpawnExplosion(Vector3 position, ItemType type = ItemType.GrenadeHE)
            => ExplosionUtils.ServerSpawnEffect(position, type);

        /// <summary>
        /// Spawns a ragdoll converted by SCP-3114.
        /// </summary>
        /// <param name="position">Where to spawn the ragdoll.</param>
        /// <param name="scale">Scale of the ragdoll.</param>
        /// <param name="rotation">Rotation of the ragdoll.</param>
        /// <returns>The spawned ragdoll.</returns>
        public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Quaternion rotation)
            => SpawnBonesRagdoll(position, scale, Vector3.zero, rotation);

        /// <summary>
        /// Spawns a ragdoll converted by SCP-3114.
        /// </summary>
        /// <param name="position">Where to spawn the ragdoll.</param>
        /// <param name="scale">Scale of the ragdoll.</param>
        /// <param name="velocity">Velocity of the player.</param>
        /// <param name="rotation">Rotation of the ragdoll.</param>
        /// <returns>The spawned ragdoll.</returns>
        public static DynamicRagdoll SpawnBonesRagdoll(Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation)
        {
            var ragdollInstance = SpawnRagdoll(RoleTypeId.Tutorial, position, scale, rotation, true, ExPlayer.Host, new UniversalDamageHandler(-1f, DeathTranslations.Warhead));

            if (ragdollInstance is null)
                throw new Exception($"Failed to spawn ragdoll.");

            var bonesRagdoll = SpawnRagdoll(RoleTypeId.Scp3114, position, scale, velocity, rotation, true, ExPlayer.Host, new Scp3114DamageHandler(ragdollInstance, false)) as DynamicRagdoll;

            Ragdolls.Remove(ragdollInstance);

            NetworkServer.Destroy(ragdollInstance.gameObject);

            if (bonesRagdoll is null)
                throw new Exception("Failed to spawn bones ragdoll.");

            Scp3114RagdollToBonesConverter.ServerConvertNew(ExPlayer.Host.Role.Scp3114, bonesRagdoll);

            ExPlayer.Host.Role.Set(RoleTypeId.None);
            return bonesRagdoll;
        }

        /// <summary>
        /// Spawns a ragdoll of a specific role.
        /// </summary>
        /// <param name="ragdollRoleType">The role to spawn a ragdoll of.</param>
        /// <param name="position">Where to spawn the ragdoll.</param>
        /// <param name="scale">Ragdoll's scale.</param>
        /// <param name="rotation">Ragdoll's rotation.</param>
        /// <param name="spawn">Whether or not to spawn it for players.</param>
        /// <param name="owner">The ragdoll's owner.</param>
        /// <param name="damageHandler">The ragdoll's cause of death.</param>
        /// <returns>The spawned ragdoll instance.</returns>
        public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale, Quaternion rotation, bool spawn = true, ExPlayer owner = null, DamageHandlerBase damageHandler = null)
            => SpawnRagdoll(ragdollRoleType, position, scale, Vector3.zero, rotation, spawn, owner, damageHandler);

        /// <summary>
        /// Spawns a ragdoll of a specific role.
        /// </summary>
        /// <param name="ragdollRoleType">The role to spawn a ragdoll of.</param>
        /// <param name="position">Where to spawn the ragdoll.</param>
        /// <param name="scale">Ragdoll's scale.</param>
        /// <param name="velocity">The player velocity.</param>
        /// <param name="rotation">Ragdoll's rotation.</param>
        /// <param name="spawn">Whether or not to spawn it for players.</param>
        /// <param name="owner">The ragdoll's owner.</param>
        /// <param name="damageHandler">The ragdoll's cause of death.</param>
        /// <returns>The spawned ragdoll instance.</returns>
        public static BasicRagdoll SpawnRagdoll(RoleTypeId ragdollRoleType, Vector3 position, Vector3 scale, Vector3 velocity, Quaternion rotation, bool spawn = true, ExPlayer owner = null, DamageHandlerBase damageHandler = null)
        {
            if (!ragdollRoleType.TryGetPrefab(out var role))
                throw new Exception($"Failed to find role prefab for role {ragdollRoleType}");

            if (role is not IRagdollRole ragdollRole)
                throw new Exception($"Role {ragdollRoleType} does not have a ragdoll.");

            damageHandler ??= new UniversalDamageHandler(-1f, DeathTranslations.Crushed);

            var ragdoll = UnityEngine.Object.Instantiate(ragdollRole.Ragdoll);

            ragdoll.NetworkInfo = new RagdollData((owner ?? ExPlayer.Host).ReferenceHub, damageHandler, ragdoll.transform.localPosition, ragdoll.transform.localRotation);

            ragdoll.transform.position = position;
            ragdoll.transform.rotation = rotation;

            ragdoll.transform.localScale = scale;

            if (ragdoll.TryGetComponent<Rigidbody>(out var rigidbody))
                rigidbody.linearVelocity = velocity;

            if (spawn)
                NetworkServer.Spawn(ragdoll.gameObject);

            Ragdolls.Add(ragdoll);
            return ragdoll;
        }

        /// <summary>
        /// Flickers light across the selected zones.
        /// </summary>
        /// <param name="duration">Flicker duration.</param>
        /// <param name="zones">The zone whitelist.</param>
        public static void FlickerLights(float duration, params FacilityZone[] zones)
        {
            foreach (var light in RoomLightController.Instances)
            {
                var room = RoomIdentifier.AllRoomIdentifiers.FirstOrDefault(r => r != null && r.LightControllers.Contains(light));

                if (room is null)
                    continue;

                if (zones.Length < 1 || zones.Contains(room.Zone))
                    light.ServerFlickerLights(duration);
            }
        }

        /// <summary>
        /// Sets color of all lights.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public static void SetLightColor(Color color)
            => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = color);

        /// <summary>
        /// Resets color of all lights.
        /// </summary>
        public static void ResetLightsColor()
            => RoomLightController.Instances.ForEach(x => x.NetworkOverrideColor = DefaultLightColor);

        /// <summary>
        /// Spawns a new item.
        /// </summary>
        /// <param name="type">Type of the item.</param>
        /// <param name="position">Where to spawn it.</param>
        /// <param name="scale">Scale of the item.</param>
        /// <param name="rotation">Rotation of the item.</param>
        /// <param name="serial">Optional item serial number.</param>
        /// <param name="spawn">Whether or not to spawn it for players.</param>
        /// <returns>The spawned item.</returns>
        public static ItemPickupBase SpawnItem(ItemType type, Vector3 position, Vector3 scale, Quaternion rotation, 
            ushort? serial = null, bool spawn = true)
            => SpawnItem<ItemPickupBase>(type, position, scale, rotation, serial, spawn);

        /// <summary>
        /// Spawns a new item.
        /// </summary>
        /// <param name="item">Type of the item.</param>
        /// <param name="position">Where to spawn it.</param>
        /// <param name="scale">Scale of the item.</param>
        /// <param name="rotation">Rotation of the item.</param>
        /// <param name="serial">Optional item serial number.</param>
        /// <param name="spawn">Whether or not to spawn it for players.</param>
        /// <typeparam name="T">Generic type of the item.</typeparam>
        /// <returns>The spawned item.</returns>
        public static T? SpawnItem<T>(ItemType item, Vector3 position, Vector3 scale, Quaternion rotation, 
            ushort? serial = null, bool spawn = true) where T : ItemPickupBase
        {
            if (!item.TryGetItemPrefab(out var prefab))
                return null;

            var pickup = UnityEngine.Object.Instantiate((T)prefab.PickupDropModel, position, rotation);

            pickup.transform.position = position;
            pickup.transform.rotation = rotation;

            pickup.transform.localScale = scale;

            pickup.Info = new PickupSyncInfo(item, prefab.Weight, serial ?? ItemSerialGenerator.GenerateNext());

            if (spawn)
            {
                NetworkServer.Spawn(pickup.gameObject);
                
                LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new ItemSpawnedEventArgs(pickup));
            }

            return pickup;
        }
        
        /// <summary>
        /// Spawns an amount of an item.
        /// </summary>
        /// <param name="type">The type of item to spawn.</param>
        /// <param name="count">The amount to spawn.</param>
        /// <param name="position">Where to spawn.</param>
        /// <param name="scale">Scale of each item.</param>
        /// <param name="rotation">Rotation of each item.</param>
        /// <param name="spawn">Whether or not to spawn.</param>
        /// <typeparam name="T">Generic type of the item.</typeparam>
        /// <returns></returns>
        public static List<T> SpawnItems<T>(ItemType type, int count, Vector3 position, Vector3 scale, Quaternion rotation, 
            bool spawn = true) where T : ItemPickupBase
        {
            var list = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                var item = SpawnItem<T>(type, position, scale, rotation, null, spawn);

                if (item is null)
                    continue;

                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Spawns a projectile.
        /// </summary>
        /// <param name="item">The projectile type.</param>
        /// <param name="position">Where to spawn it.</param>
        /// <param name="scale">Projectile scale.</param>
        /// <param name="velocity">Projectile velocity.</param>
        /// <param name="rotation">Projectile rotation.</param>
        /// <param name="force">Throwing force.</param>
        /// <param name="fuseTime">How long until detonation (in seconds).</param>
        /// <param name="spawn">Whether or not to spawn.</param>
        /// <param name="activate">Whether or not to activate.</param>
        /// <returns>The spawned projectile.</returns>
        public static ThrownProjectile SpawnProjectile(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity,
            Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true)
            => SpawnProjectile<ThrownProjectile>(item, position, scale, velocity, rotation, force, fuseTime, spawn, activate);

        /// <summary>
        /// Spawns a projectile.
        /// </summary>
        /// <param name="item">The projectile type.</param>
        /// <param name="position">Where to spawn it.</param>
        /// <param name="scale">Projectile scale.</param>
        /// <param name="velocity">Projectile velocity.</param>
        /// <param name="rotation">Projectile rotation.</param>
        /// <param name="force">Throwing force.</param>
        /// <param name="fuseTime">How long until detonation (in seconds).</param>
        /// <param name="spawn">Whether or not to spawn.</param>
        /// <param name="activate">Whether or not to activate.</param>
        /// <typeparam name="T">Generic projectile type.</typeparam>
        /// <returns>The spawned projectile.</returns>
        public static T SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 velocity, 
            Quaternion rotation, float force, float fuseTime = 2f, bool spawn = true, bool activate = true) where T : ThrownProjectile
            => SpawnProjectile<T>(item, position, scale, Vector3.forward, Vector3.up, rotation, velocity, force, fuseTime, spawn, activate);

        /// <summary>
        /// Spawns a projectile.
        /// </summary>
        /// <param name="item">Projectile type.</param>
        /// <param name="position">Spawn position.</param>
        /// <param name="scale">Projectile scale.</param>
        /// <param name="forward">Forward vector (can be <see cref="Vector3.forward"/>)</param>
        /// <param name="up">Upwards vector (can be <see cref="Vector3.up"/>)</param>
        /// <param name="rotation">Projectile rotation.</param>
        /// <param name="velocity">Projectile velocity.</param>
        /// <param name="force">Throwing force.</param>
        /// <param name="fuseTime">Time until detonation.</param>
        /// <param name="spawn">Whether or not to spawn.</param>
        /// <param name="activate">Whether or not to activate.</param>
        /// <param name="serial">Optional item serial number.</param>
        /// <typeparam name="T">Generic projectile type.</typeparam>
        /// <returns></returns>
        public static T SpawnProjectile<T>(ItemType item, Vector3 position, Vector3 scale, Vector3 forward, Vector3 up, 
            Quaternion rotation, Vector3 velocity, float force, float fuseTime = 2f, bool spawn = true, 
            bool activate = true, ushort? serial = null) where T : ThrownProjectile
        {
            if (!item.TryGetItemPrefab<ThrowableItem>(out var throwableItem))
                return null;

            var projectile = UnityEngine.Object.Instantiate((T)throwableItem.Projectile, position, rotation);
            var settings = throwableItem.FullThrowSettings;

            projectile.transform.localScale = scale;
            projectile.Info = new PickupSyncInfo(item, throwableItem.Weight, serial.HasValue ? serial.Value : ItemSerialGenerator.GenerateNext());

            settings.StartVelocity = force;
            settings.StartTorque = velocity;

            (projectile as TimeGrenade)!._fuseTime = fuseTime;

            if (spawn)
            {
                NetworkServer.Spawn(projectile.gameObject);
                LabApi.Events.Handlers.ServerEvents.OnItemSpawned(new ItemSpawnedEventArgs(projectile));
            }

            if (spawn && activate)
            {
                if (projectile.TryGetRigidbody(out var rigidbody))
                {
                    var num = 1f - Mathf.Abs(Vector3.Dot(forward, Vector3.up));
                    var vector = up * throwableItem.FullThrowSettings.UpwardsFactor;
                    var vector2 = forward + vector * num;

                    rigidbody.centerOfMass = Vector3.zero;
                    rigidbody.angularVelocity = settings.StartTorque;
                    rigidbody.linearVelocity = velocity + vector2 * force;
                }
                else
                {
                    projectile.Position = position;
                    projectile.Rotation = rotation;
                }

                projectile.ServerActivate();

                new ThrowableNetworkHandler.ThrowableItemAudioMessage(projectile.Info.Serial, ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce).SendToAuthenticated();
            }

            return projectile;
        }

        private static void OnRoundWaiting()
        {
            try
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                AmbientSoundPlayer = ExPlayer.Host.GameObject.GetComponent<AmbientSoundPlayer>();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                
                Ragdolls.Clear();
                Pickups.Clear();
                Lockers.Clear();
                Chambers.Clear();

                foreach (var locker in UnityEngine.Object.FindObjectsByType<Locker>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                {
                    Lockers.Add(locker);
                    Chambers.AddRange(locker.Chambers);
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Map API", $"Map generation failed!\n{ex.ToColoredString()}");
            }
        }

        // this is so stupid
        private static void OnIdentityDestroyed(NetworkIdentity identity)
        {
            try
            {
                if (identity is null)
                    return;
                
                Lockers.ForEach(l =>
                {
                    if (l.netId != identity.netId)
                        return;
                    
                    for (var i = 0; i < l.Chambers.Length; i++)
                        Chambers.Remove(l.Chambers[i]);
                });
                
                Lockers.RemoveAll(x => x.netId == identity.netId);

                if (identity.TryGetComponent<IInteractable>(out var interactable))
                    InteractableCollider.AllInstances.Remove(interactable);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Map API", ex);
            }
        }

        private static void OnRagdollSpawned(BasicRagdoll ragdoll)
        {
            if (ragdoll is null || !ragdoll)
                return;
            
            Ragdolls.Add(ragdoll);
        }

        private static void OnRagdollRemoved(BasicRagdoll ragdoll)
        {
            if (ragdoll is null || !ragdoll)
                return;

            Ragdolls.Remove(ragdoll);
        }

        private static void OnPickupDestroyed(ItemPickupBase pickup)
        {
            Pickups.Remove(pickup);
            
            ExPlayer.AllPlayers.ForEach(p => p.Inventory._droppedItems.Remove(pickup));
        }

        private static void OnPickupCreated(ItemPickupBase pickup)
        {
            Pickups.Add(pickup);
        }

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            RagdollManager.OnRagdollSpawned += OnRagdollSpawned;
            RagdollManager.OnRagdollRemoved += OnRagdollRemoved;

            ItemPickupBase.OnBeforePickupDestroyed += OnPickupDestroyed;
            ItemPickupBase.OnPickupAdded += OnPickupCreated;

            MirrorEvents.Destroying += OnIdentityDestroyed;

            InternalEvents.OnRoundWaiting += OnRoundWaiting;
        }
    }
}