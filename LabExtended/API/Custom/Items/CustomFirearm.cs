using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Attachments;

using LabApi.Events.Handlers;
using LabApi.Events.Arguments.PlayerEvents;

using LabExtended.Extensions;

using LabExtended.API.Custom.Items.Events;

using LabExtended.Events;
using LabExtended.Events.Player;
using LabExtended.Events.Firearms;

using LabExtended.Utilities.Firearms;

using System.ComponentModel;

using PlayerStatsSystem;

namespace LabExtended.API.Custom.Items
{
    /// <summary>
    /// Base class for custom firearms.
    /// </summary>
    public abstract class CustomFirearm : CustomItem
    {
        /// <summary>
        /// Gets or sets the firearm's maximum ammo capacity. Default values will be used if set to values lower than one.
        /// </summary>
        [Description("Sets the firearm's maximum ammo capacity. Default values will be used if set to values lower than one.")]
        public virtual int MaxAmmo { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether players can damage their allies with this firearm.
        /// </summary>
        [Description("Whether or not players can damage their allies with this firearm.")]
        public virtual bool CanDamageAllies { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether players can modify this firearm's attachments at workstations.
        /// </summary>
        [Description("Whether or not players that have this firearm can change it's attachments via workstations.")]
        public virtual bool CanChangeAttachments { get; set; } = true;

        /// <summary>
        /// Gets or sets an array of default attachments for this firearm. Random / player-defined attachments will be used if set to null.
        /// </summary>
        [Description("Sets a list of default attachments for this firearm. Random / player-defined attachments will be used if set to null.")]
        public virtual AttachmentName[]? DefaultAttachments { get; set; }

        /// <summary>
        /// Gets or sets an array of attachments which can be used with this firearm. No attachments will be allowed if set to null.
        /// </summary>
        [Description("Sets a list of attachments which can be used with this firearm. No attachments will be allowed if set to null.")]
        public virtual AttachmentName[]? WhitelistedAttachments { get; set; }

        /// <summary>
        /// Gets or sets an array of attachments which cannot be used with this firearm. All attachments will be allowed if set to null.
        /// </summary>
        [Description("Sets a list of attachments which cannot be used with this firearm. All attachments will be allowed if set to null.")]
        public virtual AttachmentName[]? BlacklistedAttachments { get; set; }

        /// <summary>
        /// Calculates the final damage value to be applied to the specified target. Allows customization of damage
        /// before it is processed.
        /// </summary>
        /// <param name="target">The target player who will receive the damage. Cannot be null.</param>
        /// <param name="damage">The initial damage value to be modified. Must be a non-negative number.</param>
        /// <returns>The modified damage value to be applied to the target. The value may be unchanged or adjusted based on
        /// custom logic.</returns>
        public virtual float ModifyDamage(ExPlayer target, float damage) 
            => damage;

        #region Event Callbacks
        /// <summary>
        /// Gets called before a player starts reloading their firearm.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnReloading(PlayerReloadingWeaponEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after a player finishes reloading their firearm.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnReloaded(PlayerReloadedWeaponEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after an animation that triggers a specific firearm method is played, but before the method is invoked.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnProcessingEvent(FirearmProcessingEventEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after an animation that triggers a specific firearm method is played, and after the method was invoked.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnProcessedEvent(FirearmProcessedEventEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called before a raycast is performed for this firearm.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnCastingRay(FirearmRayCastEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called before a firearm shot request is processed.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnShooting(PlayerShootingFirearmEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after a firearm shot request was processed.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnShot(PlayerShotFirearmEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called when an attachments change requested is received and before it is processed.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after an attachments change request was processed.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called before damage dealt by this firearm is applied.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnHurting(PlayerHurtingEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after damage dealt by this firearm is applied.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnHurt(PlayerHurtEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called before a player dies from damage dealt by this firearm.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnDying(PlayerDyingEventArgs args, ref object? firearmData)
        {

        }

        /// <summary>
        /// Gets called after a player dies from damage dealt by this firearm.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="firearmData">A reference to the firearm's custom data.</param>
        public virtual void OnDied(PlayerDeathEventArgs args, ref object? firearmData)
        {

        }
        #endregion

        /// <inheritdoc/>
        public override void OnItemAdded(CustomItemAddedEventArgs args)
        {
            base.OnItemAdded(args);

            if (args.AddedItem is Firearm firearm)
            {
                SetMaxAmmo(this, firearm);

                if (DefaultAttachments?.Length > 0)
                    firearm.SetAttachments(attachment => DefaultAttachments.Contains(attachment.Name));
            }
        }

        #region Static Event Handling
        private static void Internal_ChangingAttachments(PlayerChangingFirearmAttachmentsEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            if (!customFirearm.CanChangeAttachments)
            {
                args.IsAllowed = false;
            }
            else
            {
                if (customFirearm.WhitelistedAttachments?.Length > 0)
                {
                    args.ToEnable.RemoveAll(attachment => !customFirearm.WhitelistedAttachments.Contains(attachment));
                }

                if (customFirearm.BlacklistedAttachments?.Length > 0)
                {
                    args.ToEnable.RemoveAll(attachment => customFirearm.BlacklistedAttachments.Contains(attachment));
                }

                customFirearm.OnChangingAttachments(args, ref tracker.Data);
            }
        }

        private static void Internal_ChangedAttachments(PlayerChangedFirearmAttachmentsEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnChangedAttachments(args, ref tracker.Data);
        }

        private static void Internal_Reloading(PlayerReloadingWeaponEventArgs args)
        {
            if (args.FirearmItem?.Base == null)
                return;

            if (!IsTrackedItem(args.FirearmItem.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            if (customFirearm.MaxAmmo > 0
                && args.FirearmItem.Base.TryGetModule<AutomaticActionModule>(out var automaticActionModule)
                && automaticActionModule != null
                && automaticActionModule.AmmoStored >= customFirearm.MaxAmmo)
            {
                args.IsAllowed = false;
            }
            else
            {
                customFirearm.OnReloading(args, ref tracker.Data);
            }
        }

        private static void Internal_Reloaded(PlayerReloadedWeaponEventArgs args)
        {
            if (args.FirearmItem?.Base == null)
                return;

            if (!IsTrackedItem(args.FirearmItem.Serial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnReloaded(args, ref tracker.Data);
        }

        private static void Internal_RayCast(FirearmRayCastEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnCastingRay(args, ref tracker.Data);
        }

        private static void Internal_ProcessingEvent(FirearmProcessingEventEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnProcessingEvent(args, ref tracker.Data);
        }

        private static void Internal_ProcessedEvent(FirearmProcessedEventEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnProcessedEvent(args, ref tracker.Data);
        }

        private static void Internal_Shooting(PlayerShootingFirearmEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnShooting(args, ref tracker.Data);
        }

        private static void Internal_Shot(PlayerShotFirearmEventArgs args)
        {
            if (args.Firearm == null)
                return;

            if (!IsTrackedItem(args.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnShot(args, ref tracker.Data);
        }

        private static void Internal_Hurting(PlayerHurtingEventArgs args)
        {
            if (args.DamageHandler is not FirearmDamageHandler firearmDamageHandler)
                return;

            if (firearmDamageHandler.Firearm == null)
                return;

            if (!IsTrackedItem(firearmDamageHandler.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            var target = args.Player as ExPlayer;

            if (!customFirearm.CanDamageAllies 
                && args.Attacker is ExPlayer attacker
                && target?.ReferenceHub != null
                && attacker.Role.Type.IsFriendly(target.Role.Type))
            {
                args.IsAllowed = false;
                return;
            }
            
            if (target?.ReferenceHub != null)
                firearmDamageHandler.Damage = customFirearm.ModifyDamage(target, firearmDamageHandler.Damage);

            customFirearm.OnHurting(args, ref tracker.Data);
        }

        private static void Internal_Hurt(PlayerHurtEventArgs args)
        {
            if (args.DamageHandler is not FirearmDamageHandler firearmDamageHandler)
                return;

            if (firearmDamageHandler.Firearm == null)
                return;

            if (!IsTrackedItem(firearmDamageHandler.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnHurt(args, ref tracker.Data);
        }

        private static void Internal_Dying(PlayerDyingEventArgs args)
        {
            if (args.DamageHandler is not FirearmDamageHandler firearmDamageHandler)
                return;

            if (firearmDamageHandler.Firearm == null)
                return;

            if (!IsTrackedItem(firearmDamageHandler.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnDying(args, ref tracker.Data);
        }

        private static void Internal_Died(PlayerDeathEventArgs args)
        {
            if (args.DamageHandler is not FirearmDamageHandler firearmDamageHandler)
                return;

            if (firearmDamageHandler.Firearm == null)
                return;

            if (!IsTrackedItem(firearmDamageHandler.Firearm.ItemSerial, out var tracker))
                return;

            if (tracker.TargetItem is not CustomFirearm customFirearm)
                return;

            customFirearm.OnDied(args, ref tracker.Data);
        }
        #endregion

        internal new static void Internal_Init()
        {
            ExFirearmEvents.RayCast += Internal_RayCast;

            ExPlayerEvents.ChangingAttachments += Internal_ChangingAttachments;
            ExPlayerEvents.ChangedAttachments += Internal_ChangedAttachments;

            ExPlayerEvents.ShootingFirearm += Internal_Shooting;
            ExPlayerEvents.ShotFirearm += Internal_Shot;

            ExFirearmEvents.ProcessingEvent += Internal_ProcessingEvent;
            ExFirearmEvents.ProcessedEvent += Internal_ProcessedEvent;

            PlayerEvents.ReloadingWeapon += Internal_Reloading;
            PlayerEvents.ReloadedWeapon += Internal_Reloaded;

            PlayerEvents.Hurting += Internal_Hurting;
            PlayerEvents.Hurt += Internal_Hurt;

            PlayerEvents.Dying += Internal_Dying;
            PlayerEvents.Death += Internal_Died;
        }

        private static void SetMaxAmmo(CustomFirearm customFirearm, Firearm firearmInstance)
        {
            if (firearmInstance != null && customFirearm.MaxAmmo > 0)
            {
                if (firearmInstance.TryGetModule<MagazineModule>(out var magazineModule) && magazineModule != null)
                {
                    magazineModule._defaultCapacity = customFirearm.MaxAmmo;

                    if (firearmInstance.TryGetModule<AutomaticActionModule>(out var automaticActionModule)
                        && automaticActionModule != null
                        && automaticActionModule.AmmoStored > 0)
                        automaticActionModule.ServerCycleAction();

                    if (magazineModule.AmmoStored > customFirearm.MaxAmmo)
                        magazineModule.ServerModifyAmmo(-(magazineModule.AmmoStored - customFirearm.MaxAmmo));
                }
            }
        }
    }
}