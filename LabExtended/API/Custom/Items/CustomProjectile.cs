using InventorySystem.Items;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.ServerEvents;

using LabApi.Events.Handlers;

using LabExtended.Events;
using LabExtended.Events.Map;

using LabExtended.Extensions;

using Mirror;

using System.ComponentModel;

using UnityEngine;

namespace LabExtended.API.Custom.Items
{
    /// <summary>
    /// Base class for custom explosive projectiles.
    /// </summary>
    public abstract class CustomProjectile : CustomItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether the projectile will automatically explode upon colliding with another
        /// object.
        /// </summary>
        [Description("Whether or not the projectile should explode upon collision.")]
        public virtual bool ExplodeOnCollision { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a spawned projectile should explode when another explosion occurs near it.
        /// </summary>
        [Description("Whether or not a spawned projectile should explode when another explosion occurs near it.")]
        public virtual bool ExplodeOnExplosion { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether players are prevented from picking up thrown projectiles.
        /// </summary>
        [Description("Whether or not players should be able to pick up thrown projectiles.")]
        public virtual bool LockProjectile { get; set; } = true;

        /// <summary>
        /// Gets or sets the fuse time of the projectile (in seconds). Values below zero use the default time of the item.
        /// </summary>
        [Description("Sets the fuse time of the projectile in seconds. Values below zero use the default time of the item.")]
        public virtual float FuseTime { get; set; } = -1f;

        /// <summary>
        /// Throws a projectile from the specified position with the given velocity and force, creating and activating a
        /// new projectile instance in the game world.
        /// </summary>
        /// <param name="startPosition">The world-space position from which the projectile is thrown.</param>
        /// <param name="velocity">The initial velocity vector applied to the projectile upon being thrown.</param>
        /// <param name="throwForce">The multiplier applied to the projectile's throw force. Defaults to 1.0 if not specified.</param>
        /// <param name="projectileData">Optional custom data associated with the projectile. Can be used to pass additional context or metadata for
        /// tracking or gameplay purposes.</param>
        /// <returns>A ThrownProjectile instance representing the newly created and activated projectile.</returns>
        /// <exception cref="Exception">Thrown if the projectile template cannot be retrieved or is invalid for the current inventory type.</exception>
        public virtual ThrownProjectile ThrowProjectile(Vector3 startPosition, Vector3 velocity, float throwForce = 1f, object? projectileData = null)
        {
            if (!PickupType.TryGetItemPrefab<ThrowableItem>(out var throwableProjectileTemplate)
                || throwableProjectileTemplate == null || throwableProjectileTemplate.Projectile == null)
                throw new Exception($"[{Id} - {Name}] Could not get projectile template ({PickupType}");

            var thrownProjectile = UnityEngine.Object.Instantiate(throwableProjectileTemplate.Projectile);
            var thrownProjectileInfo = new PickupSyncInfo(InventoryType,
                Weight > 0f ? Weight : throwableProjectileTemplate.Weight, ItemSerialGenerator.GenerateNext(), LockProjectile);

            if (thrownProjectile is TimeGrenade timeGrenade && FuseTime >= 0f)
                timeGrenade._fuseTime = FuseTime;

            thrownProjectile.NetworkInfo = thrownProjectileInfo;
            thrownProjectile.PreviousOwner = ExPlayer.Host.Footprint;

            NetworkServer.Spawn(thrownProjectile.gameObject);

            thrownProjectile.InfoReceivedHook(default, thrownProjectileInfo);
            thrownProjectile.Position = startPosition;

            if (thrownProjectile.TryGetRigidbody(out var rigidbody))
                throwableProjectileTemplate.PropelBody(rigidbody, throwableProjectileTemplate.FullThrowSettings.StartTorque, ThrowableNetworkHandler.GetLimitedVelocity(velocity));

            thrownProjectile.ServerActivate();

            Internal_TrackPickup(thrownProjectile, projectileData);
            return thrownProjectile;
        }

        /// <summary>
        /// Throws a projectile using the specified ThrowableItem instance, creating and activating a new projectile instance.
        /// </summary>
        /// <param name="throwableItem">The original throwable instance.</param>
        /// <returns>The newly spawned projectile instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual ThrownProjectile ThrowProjectile(ThrowableItem throwableItem)
        {
            if (throwableItem == null)
                throw new ArgumentNullException(nameof(throwableItem));

            if (!Internal_CheckItem(throwableItem.ItemSerial, out var trackedItem))
                throw new Exception($"[{Id} - {Name}] Item {throwableItem.ItemSerial} does not belong to this CustomProjectile");

            if (!ExPlayer.TryGet(throwableItem.Owner, out var owner))
                throw new Exception($"[{Id} - {Name}] Item {throwableItem.ItemSerial} does not have a valid owner");

            var thrownProjectile = UnityEngine.Object.Instantiate(throwableItem.Projectile);
            var thrownProjectileInfo = new PickupSyncInfo(InventoryType,
                Weight > 0f ? Weight : throwableItem.Weight, throwableItem.ItemSerial, LockProjectile);

            if (thrownProjectile is TimeGrenade timeGrenade && FuseTime >= 0f)
                timeGrenade._fuseTime = FuseTime;

            thrownProjectile.NetworkInfo = thrownProjectileInfo;
            thrownProjectile.PreviousOwner = owner.Footprint;

            NetworkServer.Spawn(thrownProjectile.gameObject);

            thrownProjectile.InfoReceivedHook(default, thrownProjectileInfo);

            if (thrownProjectile.TryGetRigidbody(out var rigidbody))
                throwableItem.PropelBody(rigidbody, throwableItem.FullThrowSettings.StartTorque, ThrowableNetworkHandler.GetLimitedVelocity(owner.Velocity));

            thrownProjectile.ServerActivate();

            trackedItem.Pickup = thrownProjectile;

            trackedItem.Item = null;
            trackedItem.Owner = null;

            throwableItem.DestroyItem();
            return thrownProjectile;
        }

        /// <inheritdoc/>
        public override ItemPickupBase SpawnItem(Vector3 position, Quaternion? rotation, object? pickupData = null)
        {
            if (!PickupType.TryGetItemPrefab<ThrowableItem>(out var throwableProjectileTemplate)
                || throwableProjectileTemplate == null || throwableProjectileTemplate.Projectile == null)
                throw new Exception($"[{Id} - {Name}] Could not get projectile template ({PickupType}");

            var thrownProjectile = UnityEngine.Object.Instantiate(throwableProjectileTemplate.Projectile);
            var thrownProjectileInfo = new PickupSyncInfo(InventoryType,
                Weight > 0f ? Weight : throwableProjectileTemplate.Weight, ItemSerialGenerator.GenerateNext(), LockProjectile);

            if (thrownProjectile is TimeGrenade timeGrenade && FuseTime >= 0f)
                timeGrenade._fuseTime = FuseTime;

            thrownProjectile.Position = position;
            thrownProjectile.Rotation = rotation ?? Quaternion.identity;

            thrownProjectile.NetworkInfo = thrownProjectileInfo;
            thrownProjectile.PreviousOwner = ExPlayer.Host.Footprint;

            NetworkServer.Spawn(thrownProjectile.gameObject);

            thrownProjectile.InfoReceivedHook(default, thrownProjectileInfo);
            thrownProjectile.ServerActivate();

            Internal_TrackPickup(thrownProjectile, pickupData);
            return thrownProjectile;
        }

        /// <inheritdoc/>
        public override void OnCollided(PickupCollidedEventArgs args, ref object? pickupData)
        {
            base.OnCollided(args, ref pickupData);

            if (ExplodeOnCollision && args.Pickup is ThrownProjectile thrownProjectile)
            {
                if (thrownProjectile is TimeGrenade timeGrenade)
                    timeGrenade._fuseTime = 0.1f;

                thrownProjectile.ServerActivate();
            }
        }

        /// <summary>
        /// Gets called before a player throws a projectile.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnThrowingProjectile(PlayerThrowingProjectileEventArgs args, ref object? projectileData)
        {

        }

        /// <summary>
        /// Gets called after a player has thrown a projectile.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnThrewProjectile(PlayerThrewProjectileEventArgs args, ref object? projectileData)
        {

        }

        /// <summary>
        /// Gets called before a projectile explodes.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnExploding(ProjectileExplodingEventArgs args, ref object? projectileData)
        {

        }

        /// <summary>
        /// Gets called after a projectile explodes.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnExploded(ProjectileExplodedEventArgs args, ref object? projectileData)
        {

        }

        /// <summary>
        /// Gets called before an inactive projectile is activated by another explosion.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnActivating(ProjectileActivatingEventArgs args, ref object? projectileData)
        {

        }

        /// <summary>
        /// Gets called after an inactive projectile has been activated by another explosion (but before the projectile is spawned and activated).
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="projectileData">A reference to the projectile's custom data.</param>
        public virtual void OnActivated(ProjectileActivatedEventArgs args, ref object? projectileData)
        {

        }

        private static void Internal_Throwing(PlayerThrowingProjectileEventArgs args)
        {
            if (args.ThrowableItem?.Base == null)
                return;

            if (!IsTrackedItem(args.ThrowableItem.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            customProjectile.OnThrowingProjectile(args, ref tracker.Data);
        }

        private static void Internal_Threw(PlayerThrewProjectileEventArgs args)
        {
            if (args.ThrowableItem?.Base == null)
                return;

            if (args.Projectile?.Base == null)
                return;

            if (!IsTrackedItem(args.ThrowableItem.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            if (customProjectile.FuseTime >= 0f 
                && args.Projectile.Base is TimeGrenade timeGrenade
                && !timeGrenade._alreadyDetonated)
            {
                timeGrenade._fuseTime = customProjectile.FuseTime;
                timeGrenade.ServerActivate();
            }

            customProjectile.OnThrewProjectile(args, ref tracker.Data);
        }

        private static void Internal_Exploding(ProjectileExplodingEventArgs args)
        {
            if (args.TimedGrenade?.Base == null)
                return;

            if (!IsTrackedItem(args.TimedGrenade.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            customProjectile.OnExploding(args, ref tracker.Data);
        }

        private static void Internal_Exploded(ProjectileExplodedEventArgs args)
        {
            if (args.TimedGrenade?.Base == null)
                return;

            if (!IsTrackedItem(args.TimedGrenade.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            customProjectile.OnExploded(args, ref tracker.Data);
            customProjectile.trackers.Remove(tracker);
        }

        private static void Internal_Activating(ProjectileActivatingEventArgs args)
        {
            if (args.Pickup == null)
                return;

            if (!IsTrackedItem(args.Pickup.Info.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            if (!customProjectile.ExplodeOnExplosion)
            {
                args.IsAllowed = false;
                return;
            }

            customProjectile.OnActivating(args, ref tracker.Data);
        }

        private static void Internal_Activated(ProjectileActivatedEventArgs args)
        {
            if (args.Pickup == null)
                return;

            if (!IsTrackedItem(args.Pickup.Info.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomProjectile customProjectile)
                return;

            if (args.Projectile is TimeGrenade timeGrenade && customProjectile.FuseTime >= 0f)
                timeGrenade._fuseTime = customProjectile.FuseTime;

            customProjectile.OnActivated(args, ref tracker.Data);
        }

        internal new static void Internal_Init()
        {
            PlayerEvents.ThrowingProjectile += Internal_Throwing;
            PlayerEvents.ThrewProjectile += Internal_Threw;

            ServerEvents.ProjectileExploding += Internal_Exploding;
            ServerEvents.ProjectileExploded += Internal_Exploded;

            ExMapEvents.ProjectileActivating += Internal_Activating;
            ExMapEvents.ProjectileActivated += Internal_Activated;
        }
    }
}