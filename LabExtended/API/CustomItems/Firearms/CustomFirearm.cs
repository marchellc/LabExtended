using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.Events.Other;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using PlayerRoles.Spectating;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.API.CustomItems.Firearms
{
    public class CustomFirearm : CustomItem
    {
        public CustomFirearmInfo FirearmInfo => (CustomFirearmInfo)Info;

        public Firearm FirearmItem
        {
            get
            {
                if (Item != null && Item is Firearm firearm)
                    return firearm;

                return null;
            }
        }

        public FirearmPickup FirearmPickup
        {
            get
            {
                if (Pickup != null && Pickup is FirearmPickup firearmPickup)
                    return firearmPickup;

                return null;
            }
        }

        public byte CurrentAmmo
        {
            get
            {
                if (FirearmItem != null)
                    return FirearmItem.Status.Ammo;

                if (FirearmPickup != null)
                    return FirearmPickup.Status.Ammo;

                return 0;
            }
            set
            {
                FirearmItem!.Status = new FirearmStatus(value, FirearmItem.Status.Flags, FirearmItem.Status.Attachments);
                FirearmPickup!.NetworkStatus = new FirearmStatus(value, FirearmPickup.Status.Flags, FirearmPickup.Status.Attachments);
            }
        }

        public ItemType AmmoType => FirearmInfo.AmmoType;

        public byte MaxAmmo => FirearmInfo.MaxAmmo;
        public byte StartingAmmo => FirearmInfo.StartAmmo;

        public bool HasUnlimitedAmmo => (FirearmInfo.FirearmFlags & CustomFirearmFlags.UnlimitedAmmo) == CustomFirearmFlags.UnlimitedAmmo;

        public virtual void OnRequestReceived(ProcessingFirearmRequestArgs args) { }
        public virtual void OnRequestProcessed(ProcessedFirearmRequestArgs args) { }

        public virtual void OnAuthorizingShot(PlayerAuthorizingShotArgs args) { }
        public virtual void OnPerformingShot(PlayerPerformingShotArgs args) { }
        public virtual void OnProcessingShot(ProcessingFirearmShotArgs args) { }

        public virtual bool OnReloading(ref byte ammoToReload) => true;
        public virtual void OnReloaded(byte addedAmmo) { }

        public virtual bool OnStoppingReload() => true;
        public virtual void OnStoppedReload() { }

        public virtual bool OnUnloading() => true;
        public virtual void OnUnloaded() { }

        public virtual bool OnDryFiring() => true;
        public virtual void OnDryFired() { }

        public virtual bool OnZoomingIn() => true;
        public virtual void OnZoomedIn() { }

        public virtual bool OnZoomingOut() => true;
        public virtual void OnZoomedOut() { }

        public virtual bool OnTogglingFlashlight(bool isEnabled) => true;
        public virtual void OnToggledFlashlight(bool isEnabled) { }

        public virtual bool OnInspecting() => true;
        public virtual void OnInspected() { }

        internal override void SetupItem()
        {
            FirearmItem.Status = new FirearmStatus(StartingAmmo, FirearmItem.Status.Flags | FirearmStatusFlags.MagazineInserted, FirearmItem.Status.Attachments);
            FirearmItem.ApplyPreferences(Owner);
        }

        internal bool InternalCanReload()
            => AmmoType != ItemType.None && (!FirearmInfo.FirearmFlags.Any(CustomFirearmFlags.AmmoAsInventoryItems) ? Owner.GetAmmo(AmmoType) > 0 : Owner.CountItems(AmmoType) > 0);

        internal void InternalReload(byte ammoToReload)
        {
            if (AmmoType is ItemType.None)
                return;

            if (FirearmInfo.FirearmFlags.Any(CustomFirearmFlags.AmmoAsInventoryItems) && !AmmoType.IsAmmo())
            {
                var availableAmmo = (byte)Owner.CountItems(AmmoType);

                if (availableAmmo < 1)
                    return;

                if (availableAmmo > ammoToReload)
                    availableAmmo = ammoToReload;

                if (!OnReloading(ref availableAmmo))
                    return;

                Owner.RemoveItems(AmmoType, availableAmmo);
                CurrentAmmo += availableAmmo;

                new RequestMessage(Serial, RequestType.Reload).SendToAuthenticated();

                OnReloaded(availableAmmo);
            }
            else
            {
                var availableAmmo = (byte)Mathf.Clamp(Owner.GetAmmo(AmmoType), 0f, ammoToReload);

                if (availableAmmo < 1)
                    return;

                if (availableAmmo > ammoToReload)
                    availableAmmo = ammoToReload;

                if (!OnReloading(ref availableAmmo))
                    return;

                Owner.SubstractAmmo(AmmoType, availableAmmo);
                CurrentAmmo += availableAmmo;

                new RequestMessage(Serial, RequestType.Reload).SendToAuthenticated();

                OnReloaded(availableAmmo);
            }
        }

        internal static void InternalOnProcessingShot(ProcessingFirearmShotArgs args)
        {
            if (!TryGetItem<CustomFirearm>(args.Firearm, out var customFirearm))
                return;

            customFirearm.OnProcessingShot(args);
        }

        internal static void InternalOnAuthorizingShot(PlayerAuthorizingShotArgs shotArgs)
        {
            if (!TryGetItem<CustomFirearm>(shotArgs.Firearm, out var customFirearm))
                return;

            if (customFirearm.HasUnlimitedAmmo)
                shotArgs.SubstractAmmo = 0;

            customFirearm.OnAuthorizingShot(shotArgs);
        }

        internal static void InternalOnPerformingShot(PlayerPerformingShotArgs args)
        {
            if (!TryGetItem<CustomFirearm>(args.Firearm, out var customFirearm))
                return;

            customFirearm.OnPerformingShot(args);
        }

        internal static void InternalOnProcessedRequested(ProcessedFirearmRequestArgs args)
        {
            if (!TryGetItem<CustomFirearm>(args.Message.Serial, out var customFirearm))
                return;

            customFirearm.OnRequestProcessed(args);
        }

        internal static bool InternalOnProcessingRequest(ProcessingFirearmRequestArgs processingFirearmRequestArgs)
        {
            if (!TryGetItem<CustomFirearm>(processingFirearmRequestArgs.Message.Serial, out var customFirearm))
                return true;

            switch (processingFirearmRequestArgs.Message.Request)
            {
                case RequestType.Reload:
                    {
                        if (!customFirearm.InternalCanReload())
                            return false;

                        customFirearm.InternalReload((byte)(customFirearm.MaxAmmo - customFirearm.CurrentAmmo));
                        return false;
                    }

                case RequestType.Unload:
                    {
                        if (!customFirearm.OnUnloading())
                            return false;

                        processingFirearmRequestArgs.Message.SendToAuthenticated();
                        customFirearm.OnUnloaded();

                        return false;
                    }

                case RequestType.ReloadStop:
                    {
                        if (!customFirearm.OnStoppingReload())
                            return false;

                        processingFirearmRequestArgs.Message.SendToAuthenticated();
                        customFirearm.OnStoppedReload();

                        return false;
                    }

                case RequestType.Dryfire:
                    {
                        if (!customFirearm.OnDryFiring())
                            return false;

                        processingFirearmRequestArgs.Message.SendToAuthenticated();
                        customFirearm.OnDryFired();

                        return false;
                    }

                case RequestType.AdsIn:
                    {
                        if (!customFirearm.OnZoomingIn())
                            return false;

                        processingFirearmRequestArgs.Message.SendToAuthenticated();
                        customFirearm.OnZoomedIn();

                        return false;
                    }

                case RequestType.AdsOut:
                    {
                        if (!customFirearm.OnZoomingOut())
                            return false;

                        processingFirearmRequestArgs.Message.SendToAuthenticated();
                        customFirearm.OnZoomedOut();

                        return false;
                    }

                case RequestType.ToggleFlashlight:
                    {
                        if (!customFirearm.FirearmItem.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
                            return false;

                        var isEnabled = customFirearm.FirearmItem.HasFlashlightEnabled();

                        if (!customFirearm.OnTogglingFlashlight(!isEnabled))
                            return false;

                        if (!isEnabled)
                            customFirearm.FirearmItem.DisableFlashlight();
                        else
                            customFirearm.FirearmItem.EnableFlashlight();

                        customFirearm.OnToggledFlashlight(!isEnabled);
                        return false;
                    }

                case RequestType.Inspect:
                    {
                        if (!customFirearm.OnInspecting())
                            return false;

                        processingFirearmRequestArgs.Message.SendToHubsConditionally(hub => hub.roleManager.CurrentRole is SpectatorRole);
                        customFirearm.OnInspected();

                        return false;
                    }
            }

            return true;
        }
    }
}