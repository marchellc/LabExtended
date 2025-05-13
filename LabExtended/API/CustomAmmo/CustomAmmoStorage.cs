using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.CustomAmmo;

/// <summary>
/// Used to store custom ammo.
/// </summary>
public class CustomAmmoStorage : IDisposable
{
    /// <summary>
    /// Gets the dictionary used to store custom ammo.
    /// </summary>
    public Dictionary<ushort, int> Ammo { get; private set; } = DictionaryPool<ushort, int>.Shared.Rent();

    /// <summary>
    /// Gets called once the ammo list is modified.
    /// </summary>
    public event Action? Modified;
    
    /// <summary>
    /// Gets the amount of stored ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <returns>The amount of stored ammo.</returns>
    public int Get(ushort ammoId)
    {
        if (!Ammo.TryGetValue(ammoId, out var value))
            return 0;

        return value;
    }

    /// <summary>
    /// Sets the amount of stored ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <param name="amount">The amount of the ammo.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Set(ushort ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");
        
        Ammo[ammoId] = amount;
        
        Modified?.InvokeSafe();
    }

    /// <summary>
    /// Adds the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID to add.</param>
    /// <param name="amount">The amount to add.</param>
    /// <returns>The currently stored amount of ammo.</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public int Add(ushort ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");

        if (!Ammo.ContainsKey(ammoId))
        {
            Ammo.Add(ammoId, amount);
            
            Modified?.InvokeSafe();
            return amount;
        }

        var newAmount = Ammo[ammoId] += amount;
       
        Modified?.InvokeSafe();
        return newAmount;
    }

    /// <summary>
    /// Removes the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ID of the ammo.</param>
    /// <param name="amount">The amount to remove.</param>
    /// <returns>The currently stored amount of ammo.</returns>
    public int Remove(ushort ammoId, int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Value must be non-negative");

        if (!Ammo.ContainsKey(ammoId))
            return 0;

        var current = Ammo[ammoId] -= ammoId;

        if (current <= 0)
        {
            Ammo.Remove(ammoId);
            
            Modified?.InvokeSafe();
            return 0;
        }
        
        Modified?.InvokeSafe();
        return current;
    }

    /// <summary>
    /// Whether or not the player has at least one bullet of the specified ammo stored.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <returns>true if the player has at least one bullet</returns>
    public bool HasAny(ushort ammoId)
        => Get(ammoId) > 0;
    
    /// <summary>
    /// Whether or not the player has at least the specified amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <param name="amount">The required amount.</param>
    /// <returns>true if the player has at least the specified amount</returns>
    public bool HasAtLeast(ushort ammoId, int amount)
        => Get(ammoId) >= amount;

    /// <summary>
    /// Whether or not the player has exactly this amount of ammo.
    /// </summary>
    /// <param name="ammoId">The ammo ID.</param>
    /// <param name="amount">The required amount.</param>
    /// <returns>true if the player has exactly the specified amount</returns>
    public bool HasExactly(ushort ammoId, int amount)
        => Get(ammoId) == amount;

    /// <summary>
    /// Clears all ammo.
    /// </summary>
    public void Clear()
    {
        Ammo.Clear();
        Modified?.InvokeSafe();
    }

    /// <inheritdoc cref="CustomAmmoStorage"/>
    public void Dispose()
    {
        if (Ammo != null)
            DictionaryPool<ushort, int>.Shared.Return(Ammo);
        
        Ammo = null;
        Modified = null;
    }
}