using InventorySystem;
using InventorySystem.Items.Firearms.Ammo;

using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.API.Containers;

/// <summary>
/// Used to manage inventory ammo.
/// </summary>
public class AmmoContainer
{
    /// <summary>
    /// Gets a list of all ammo types.
    /// </summary>
    public static IReadOnlyList<ItemType> AmmoTypes { get; } =
        EnumUtils<ItemType>.Values.Where(x => x.IsAmmo()).ToList();

    internal AmmoContainer(Inventory inv)
        => Inventory = inv;

    /// <summary>
    /// Gets the player's <see cref="InventorySystem.Inventory"/> component.
    /// </summary>
    public Inventory Inventory { get; }

    /// <summary>
    /// Gets the player's ammo.
    /// </summary>
    public Dictionary<ItemType, ushort> Ammo => Inventory.UserInventory.ReserveAmmo;

    /// <summary>
    /// Gets the dictionary used to store custom ammo.
    /// </summary>
    public Dictionary<string, int> CustomAmmo { get; } = new();

    /// <summary>
    /// Whether or not the player has any ammo at all.
    /// </summary>
    public bool HasAnyAmmo => Inventory.UserInventory.ReserveAmmo.Any(p => p.Value > 0);

    /// <summary>
    /// Gets or sets the amount of 12 gauge ammo in player's inventory.
    /// </summary>
    public ushort Ammo12Gauge
    {
        get => GetAmmo(ItemType.Ammo12gauge);
        set => SetAmmo(ItemType.Ammo12gauge, value);
    }

    /// <summary>
    /// Gets or sets the amount of 44cal ammo in player's inventory.
    /// </summary>
    public ushort Ammo44Cal
    {
        get => GetAmmo(ItemType.Ammo44cal);
        set => SetAmmo(ItemType.Ammo44cal, value);
    }

    /// <summary>
    /// Gets or sets the amount of 9mm ammo in player's inventory.
    /// </summary>
    public ushort Ammo9x19
    {
        get => GetAmmo(ItemType.Ammo9x19);
        set => SetAmmo(ItemType.Ammo9x19, value);
    }

    /// <summary>
    /// Gets or sets the amount of 5.56mm ammo in player's inventory.
    /// </summary>
    public ushort Ammo556x45
    {
        get => GetAmmo(ItemType.Ammo556x45);
        set => SetAmmo(ItemType.Ammo556x45, value);
    }

    /// <summary>
    /// Gets or sets the amount of 7.62mm ammo in player's inventory.
    /// </summary>
    public ushort Ammo762x39
    {
        get => GetAmmo(ItemType.Ammo762x39);
        set => SetAmmo(ItemType.Ammo762x39, value);
    }

    /// <summary>
    /// Gets called once the custom ammo list is modified.
    /// </summary>
    public event Action? CustomAmmoModified;

    /// <summary>
    /// Gets the amount of specified ammo type in player's inventory.
    /// </summary>
    /// <param name="ammoType">The ammo type to get.</param>
    /// <returns>The amount of ammo.</returns>
    public ushort GetAmmo(ItemType ammoType)
        => Ammo.TryGetValue(ammoType, out var amount) ? amount : (ushort)0;

    /// <summary>
    /// Sets the amount of specified ammo type in player's inventory.
    /// </summary>
    /// <param name="ammoType">The type of ammo.</param>
    /// <param name="amount">The ammount of ammo.</param>
    public void SetAmmo(ItemType ammoType, ushort amount)
    {
        Ammo[ammoType] = amount;
        Inventory.SendAmmoNextFrame = true;
    }

    /// <summary>
    /// Adds the specified amount of ammo.
    /// </summary>
    /// <param name="ammoType">The type of ammo to add.</param>
    /// <param name="amount">The amount of ammo to add.</param>
    public void AddAmmo(ItemType ammoType, ushort amount)
    {
        Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) + amount, 0f, ushort.MaxValue);
        Inventory.SendAmmoNextFrame = true;
    }

    /// <summary>
    /// Removes the specified amount of ammo.
    /// </summary>
    /// <param name="ammoType">The type of ammo to remove.</param>
    /// <param name="amount">The amount of ammo to remove.</param>
    public void SubstractAmmo(ItemType ammoType, ushort amount)
    {
        Ammo[ammoType] = (ushort)Mathf.Clamp(GetAmmo(ammoType) - amount, 0f, ushort.MaxValue);
        Inventory.SendAmmoNextFrame = true;
    }

    /// <summary>
    /// Gets a value indicating whether the player has the required amount of ammo.
    /// </summary>
    /// <param name="ammoType">The type of ammo to count.</param>
    /// <param name="minAmount">The minimum required amount of ammo.</param>
    /// <returns><see langword="true"/> if the player has at least <see cref="minAmount"/> of ammo, otherwise <see langword="false"/></returns>
    public bool HasAmmo(ItemType ammoType, ushort minAmount = 1)
        => GetAmmo(ammoType) >= minAmount;

    /// <summary>
    /// Gets the player's reserve ammo.
    /// </summary>
    /// <param name="ammoType">The type of reserve ammo.</param>
    /// <param name="reserveAmmo">The retrieved amount of reserve ammo.</param>
    /// <returns>true if the reserve ammo was retrieved</returns>
    public bool TryGetReserveAmmo(ItemType ammoType, out int reserveAmmo)
        => ReserveAmmoSync.TryGet(Inventory._hub, ammoType, out reserveAmmo);

    /// <summary>
    /// Sets the player's reserve ammo.
    /// </summary>
    /// <param name="ammoType">The ammo type.</param>
    /// <param name="reserveAmmo">The amount of reserve ammo.</param>
    public void SetReserveAmmo(ItemType ammoType, int reserveAmmo)
        => ReserveAmmoSync.Set(Inventory._hub, ammoType, reserveAmmo);

    /// <summary>
    /// Removes all the player's ammo.
    /// </summary>
    public void ClearAmmo()
    {
        Ammo.Clear();
        Inventory.SendAmmoNextFrame = true;
    }

    /// <summary>
    /// Removes all ammo of specified type.
    /// </summary>
    /// <param name="ammoType">The ammo type to remove.</param>
    public void ClearAmmo(ItemType ammoType)
    {
        if (Ammo.Remove(ammoType))
            Inventory.SendAmmoNextFrame = true;
    }

    /// <summary>
    /// Drops all the player's ammo.
    /// </summary>
    /// <returns>A list of spawned ammo pickups.</returns>
    public List<AmmoPickup> DropAllAmmo()
    {
        var droppedAmmo = new List<AmmoPickup>();

        foreach (var ammo in Ammo.Keys)
            droppedAmmo.AddRange(Inventory.ServerDropAmmo(ammo, ushort.MaxValue));

        return droppedAmmo;
    }

    /// <summary>
    /// Drops the specified amount of ammo.
    /// </summary>
    /// <param name="ammoType">The ammo type to drop.</param>
    /// <param name="amount">The amount of ammo to drop.</param>
    /// <returns>A list of spawned ammo pickups.</returns>
    public List<AmmoPickup> DropAllAmmo(ItemType ammoType, ushort amount = ushort.MaxValue)
        => Inventory.ServerDropAmmo(ammoType, amount);

    /// <summary>
    /// Gets the amount of stored ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <returns>The amount of stored ammo.</returns>
    public int GetCustomAmmo(string ammoId)
    {
        if (!CustomAmmo.TryGetValue(ammoId, out var value))
            return 0;

        return value;
    }

    /// <summary>
    /// Sets the amount of stored ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <param name="amount">The amount of the ammo.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void SetCustomAmmo(string ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");

        CustomAmmo[ammoId] = amount;
        CustomAmmoModified?.InvokeSafe();
    }

    /// <summary>
    /// Adds the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID to add.</param>
    /// <param name="amount">The amount to add.</param>
    /// <returns>The currently stored amount of ammo.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int AddCustomAmmo(string ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");

        if (!CustomAmmo.ContainsKey(ammoId))
        {
            CustomAmmo.Add(ammoId, amount);
            CustomAmmoModified?.InvokeSafe();

            return amount;
        }

        var newAmount = CustomAmmo[ammoId] += amount;

        CustomAmmoModified?.InvokeSafe();
        return newAmount;
    }

    /// <summary>
    /// Removes the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <param name="amount">The amount to remove.</param>
    /// <returns>The currently stored amount of ammo.</returns>
    public int RemoveCustomAmmo(string ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");

        if (!CustomAmmo.ContainsKey(ammoId))
            return 0;

        var current = CustomAmmo[ammoId] -= amount;

        if (current <= 0)
        {
            CustomAmmo.Remove(ammoId);
            CustomAmmoModified?.InvokeSafe();

            return 0;
        }

        CustomAmmoModified?.InvokeSafe();
        return current;
    }

    /// <summary>
    /// Whether or not the player has at least one bullet of the specified ammo stored.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <returns>true if the player has at least one bullet</returns>
    public bool HasAnyCustomAmmo(string ammoId)
        => GetCustomAmmo(ammoId) > 0;

    /// <summary>
    /// Whether or not the player has at least the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <param name="amount">The required amount.</param>
    /// <returns>true if the player has at least the specified amount</returns>
    public bool HasAtLeastCustomAmmo(string ammoId, int amount)
        => GetCustomAmmo(ammoId) >= amount;

    /// <summary>
    /// Whether or not the player has exactly this amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <param name="amount">The required amount.</param>
    /// <returns>true if the player has exactly the specified amount</returns>
    public bool HasExactlyCustomAmmo(string ammoId, int amount)
        => GetCustomAmmo(ammoId) == amount;

    /// <summary>
    /// Clears all ammo.
    /// </summary>
    public void ClearCustomAmmo()
    {
        Ammo.Clear();
        CustomAmmoModified?.InvokeSafe();
    }
}