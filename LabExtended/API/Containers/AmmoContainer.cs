using InventorySystem;
using InventorySystem.Items.Firearms.Ammo;

using UnityEngine;

namespace LabExtended.API.Containers
{
    public class AmmoContainer
    {
        public AmmoContainer(Inventory inv)
            => Inventory = inv;

        public Inventory Inventory { get; }

        public Dictionary<ItemType, ushort> Ammo => Inventory.UserInventory.ReserveAmmo;

        public bool HasAnyAmmo => Inventory.UserInventory.ReserveAmmo.Any(p => p.Value > 0);

        public ushort Ammo12Gauge
        {
            get => GetAmmo(ItemType.Ammo12gauge);
            set => SetAmmo(ItemType.Ammo12gauge, value);
        }

        public ushort Ammo44Cal
        {
            get => GetAmmo(ItemType.Ammo44cal);
            set => SetAmmo(ItemType.Ammo44cal, value);
        }

        public ushort Ammo9x19
        {
            get => GetAmmo(ItemType.Ammo9x19);
            set => SetAmmo(ItemType.Ammo9x19, value);
        }

        public ushort Ammo556x45
        {
            get => GetAmmo(ItemType.Ammo556x45);
            set => SetAmmo(ItemType.Ammo556x45, value);
        }

        public ushort Ammo762x39
        {
            get => GetAmmo(ItemType.Ammo762x39);
            set => SetAmmo(ItemType.Ammo762x39, value);
        }

        public ushort GetAmmo(ItemType ammoType)
            => Ammo.TryGetValue(ammoType, out var amount) ? amount : (ushort)0;

        public void SetAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = amount;
            Inventory.SendAmmoNextFrame = true;
        }

        public void AddAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) + amount, 0f, ushort.MaxValue);
            Inventory.SendAmmoNextFrame = true;
        }
        public void SubstractAmmo(ItemType ammoType, ushort amount)
        {
            Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) - amount, 0f, ushort.MaxValue);
            Inventory.SendAmmoNextFrame = true;
        }

        public bool HasAmmo(ItemType itemType, ushort minAmount = 1)
            => GetAmmo(itemType) >= minAmount;

        public void ClearAmmo()
        {
            Ammo.Clear();
            Inventory.SendAmmoNextFrame = true;
        }

        public void ClearAmmo(ItemType ammoType)
        {
            if (Ammo.Remove(ammoType))
                Inventory.SendAmmoNextFrame = true;
        }

        public List<AmmoPickup> DropAllAmmo()
        {
            var droppedAmmo = new List<AmmoPickup>();

            foreach (var ammo in Ammo.Keys)
                droppedAmmo.AddRange(Inventory.ServerDropAmmo(ammo, ushort.MaxValue));

            return droppedAmmo;
        }

        public List<AmmoPickup> DropAllAmmo(ItemType ammoType, ushort amount = ushort.MaxValue)
            => Inventory.ServerDropAmmo(ammoType, amount);
    }
}