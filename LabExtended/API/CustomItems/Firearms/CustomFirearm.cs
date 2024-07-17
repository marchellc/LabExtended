using InventorySystem.Items.Firearms;
using InventorySystem.Items.Firearms.Attachments;
using InventorySystem.Items.Firearms.BasicMessages;

using LabExtended.Events.Other;
using LabExtended.Extensions;

using PlayerRoles.Spectating;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.API.CustomItems.Firearms
{
    public class CustomFirearm : CustomItem
    {
        public CustomFirearmInfo FirearmInfo => (CustomFirearmInfo)Info;

        public Firearm Firearm
        {
            get
            {
                if (Item != null && Item is Firearm firearm)
                    return firearm;

                return null;
            }
        }

        public byte CurrentAmmo
        {
            get => Firearm?.Status.Ammo ?? 0;
            set => Firearm!.Status = new FirearmStatus(value, Firearm.Status.Flags, Firearm.Status.Attachments);
        }

        public ItemType AmmoType => FirearmInfo.AmmoType;

        public byte MaxAmmo => FirearmInfo.MaxAmmo;
        public byte StartingAmmo => FirearmInfo.StartAmmo;

        public virtual void OnRequestReceived(ProcessingFirearmRequestArgs args) { }
        public virtual void OnRequestProcessed(ProcessedFirearmRequestArgs args) { }

        public virtual bool OnReloading(ref int ammoToReload) => true;
        public virtual void OnReloaded(int addedAmmo) { }

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
            Firearm.Status = new FirearmStatus(StartingAmmo, Firearm.Status.Flags | FirearmStatusFlags.MagazineInserted, Firearm.Status.Attachments);
            Firearm.ApplyPreferences(Owner);
        }

        internal bool InternalCanReload()
            => AmmoType != ItemType.None && (!FirearmInfo.FirearmFlags.Any(CustomFirearmFlags.AmmoAsInventoryItems) ? Owner.GetAmmo(AmmoType) > 0 : Owner.CountItems(AmmoType) > 0);

        internal void InternalReload(int ammoToReload)
        {
            if (AmmoType is ItemType.None)
                return;

            OnReloading(ref ammoToReload);

            if (FirearmInfo.FirearmFlags.Any(CustomFirearmFlags.AmmoAsInventoryItems))
            {
                var reloadableAmmo = Owner.CountItems(AmmoType);

                if (reloadableAmmo < 1)
                    return;

                ammoToReload = Mathf.Clamp(ammoToReload, ammoToReload, reloadableAmmo);

                Owner.Hub.RemoveItems(AmmoType, ammoToReload);
                CurrentAmmo += (byte)ammoToReload;

                new RequestMessage(Serial, RequestType.Reload).SendToAuthenticated();

                OnReloaded(ammoToReload);
            }
            else
            {
                var reloadableAmmo = Owner.GetAmmo(AmmoType);

                if (reloadableAmmo < 1)
                    return;

                ammoToReload = Mathf.Clamp(ammoToReload, ammoToReload, reloadableAmmo);

                Owner.SubstractAmmo(AmmoType, (ushort)ammoToReload);
                CurrentAmmo += (byte)ammoToReload;

                new RequestMessage(Serial, RequestType.Reload).SendToAuthenticated();

                OnReloaded(ammoToReload);
            }
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

                        customFirearm.InternalReload(customFirearm.MaxAmmo - customFirearm.CurrentAmmo);
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
                        if (!customFirearm.Firearm.HasAdvantageFlag(AttachmentDescriptiveAdvantages.Flashlight))
                            return false;

                        var isEnabled = customFirearm.Firearm.HasFlashlightEnabled();

                        if (!customFirearm.OnTogglingFlashlight(!isEnabled))
                            return false;

                        if (!isEnabled)
                            customFirearm.Firearm.DisableFlashlight();
                        else
                            customFirearm.Firearm.EnableFlashlight();

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