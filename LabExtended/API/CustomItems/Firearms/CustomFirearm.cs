using InventorySystem.Items.Firearms;

using LabExtended.Extensions;

using UnityEngine;

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

        internal override void SetupItem()
        {
            Firearm.Status = new FirearmStatus(StartingAmmo, Firearm.Status.Flags | FirearmStatusFlags.MagazineInserted, Firearm.Status.Attachments);
            Firearm.ApplyPreferences(Owner);
        }

        internal bool InternalCanReload()
            => AmmoType != ItemType.None && (AmmoType.IsAmmo() ? Owner.GetAmmo(AmmoType) > 0 : Owner.CountItems(AmmoType) > 0);

        internal void InternalReload()
        {
            if (AmmoType is ItemType.None)
                return;

            if (AmmoType.IsAmmo())
            {
                var ammoToReload = MaxAmmo - CurrentAmmo;
                var reloadableAmmo = Owner.GetAmmo(AmmoType);

                if (reloadableAmmo < 1)
                    return;

                ammoToReload = Mathf.Clamp(ammoToReload, ammoToReload, reloadableAmmo);

                Owner.SubstractAmmo(AmmoType, (ushort)ammoToReload);
                CurrentAmmo += (byte)ammoToReload;
            }
            else
            {
                var ammoToReload = MaxAmmo - CurrentAmmo;
                var reloadableAmmo = Owner.CountItems(AmmoType);

                if (reloadableAmmo < 1)
                    return;

                ammoToReload = Mathf.Clamp(ammoToReload, ammoToReload, reloadableAmmo);

                Owner.Hub.RemoveItems(AmmoType, ammoToReload);
                CurrentAmmo += (byte)ammoToReload;
            }
        }
    }
}