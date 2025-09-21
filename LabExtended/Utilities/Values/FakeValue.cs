using LabExtended.API;

namespace LabExtended.Utilities.Values;
/// <summary>
/// Represents a value that can be faked to other players.
/// </summary>
/// <typeparam name="T"></typeparam>
public class FakeValue<T>
{
    private readonly Dictionary<uint, T> _values = new();

    private T globalValue;
    private bool hasGlobalValue;

    /// <summary>
    /// Gets or sets the global fake value.
    /// </summary>
    /// <remarks>Will throw an exception if <see cref="HasGlobalValue"/> is <see langword="false"/>.</remarks>
    public T GlobalValue
    {
        get
        {
            if (!hasGlobalValue)
                throw new Exception("Global Value has not been set.");

            return globalValue;
        }
        set
        {
            globalValue = value;
            hasGlobalValue = true;
        }
    }

    /// <summary>
    /// Removes the global fake value.
    /// </summary>
    public void ResetGlobalValue()
    {
        hasGlobalValue = false;
        globalValue = default!;
    }

    /// <summary>
    /// Whether or not all values should be cleared when the player dies.
    /// </summary>
    public bool KeepOnDeath { get; set; }

    /// <summary>
    /// Whether or not all values should be cleared when the player's role changes.
    /// </summary>
    public bool KeepOnRoleChange { get; set; }

    /// <summary>
    /// Whether or not the global value should be cleared when the player's role changes.
    /// </summary>
    public bool KeepGlobalOnRoleChange { get; set; }

    /// <summary>
    /// Whether or not a global value has been set.
    /// </summary>
    public bool HasGlobalValue => hasGlobalValue;

    /// <summary>
    /// Gets or sets faked value for a specific network ID.
    /// </summary>
    /// <param name="netId">The network ID to set the value for.</param>
    /// <returns>The value faked for the provided network ID.</returns>
    public T this[uint netId]
    {
        get => _values[netId];
        set => _values[netId] = value;
    }

    /// <summary>
    /// Gets or sets faked value for a specific player.
    /// </summary>
    /// <param name="player">The player to set the value for.</param>
    /// <returns>The value faked for the provided player.</returns>
    public T this[ExPlayer player]
    {
        get => _values[player.NetworkId];
        set => _values[player.NetworkId] = value;
    }

    /// <summary>
    /// Gets the faked value for a specific network ID.
    /// </summary>
    /// <param name="netId">The target network ID.</param>
    /// <param name="defaultValue">The value to return if no fake value was set.</param>
    /// <returns>The faked value (if set), otherwise <paramref name="defaultValue"/></returns>
    public T? GetValue(uint netId, T? defaultValue = default)
        => _values.TryGetValue(netId, out var fakedValue) ? fakedValue : defaultValue;

    /// <summary>
    /// Gets the faked value for a player.
    /// </summary>
    /// <param name="hub">The target player.</param>
    /// <param name="defaultValue">The value to return if no fake value was set.</param>
    /// <returns>The faked value (if set), otherwise <paramref name="defaultValue"/></returns>
    public T? GetValue(ReferenceHub hub, T? defaultValue = default)
        => _values.TryGetValue(hub.netId, out var fakedValue) ? fakedValue : defaultValue;

    /// <summary>
    /// Gets the faked value for a player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="defaultValue">The value to return if no fake value was set.</param>
    /// <returns>The faked value (if set), otherwise <paramref name="defaultValue"/></returns>
    public T? GetValue(ExPlayer player, T? defaultValue = default)
        => _values.TryGetValue(player.NetworkId, out var fakedValue) ? fakedValue : defaultValue;

    /// <summary>
    /// Gets the faked value for a network ID and sets it to the referenced variable.
    /// </summary>
    /// <param name="netId">The target network ID.</param>
    /// <param name="value">The reference to the variable to set.</param>
    /// <returns><see langword="true"/> if the value of the variable was changed</returns>
    public bool GetValue(uint netId, ref T value)
    {
        if (_values.TryGetValue(netId, out var fakedValue))
        {
            value = fakedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the faked value for a player and sets it to the referenced variable.
    /// </summary>
    /// <param name="hub">The target player.</param>
    /// <param name="value">The reference to the variable to set.</param>
    /// <returns><see langword="true"/> if the value of the variable was changed</returns>
    public bool GetValue(ReferenceHub hub, ref T value)
    {
        if (_values.TryGetValue(hub.netId, out var fakedValue))
        {
            value = fakedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the faked value for a player and sets it to the referenced variable.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="value">The reference to the variable to set.</param>
    /// <returns><see langword="true"/> if the value of the variable was changed</returns>
    public bool GetValue(ExPlayer player, ref T value)
    {
        if (_values.TryGetValue(player.NetworkId, out var fakedValue))
        {
            value = fakedValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to retrieve a fake value for a specific network ID.
    /// </summary>
    /// <param name="netId">The target network ID.</param>
    /// <param name="fakedValue">The retrieved fake value.</param>
    /// <returns><see langword="true"/> if a faked value was found</returns>
    public bool TryGetValue(uint netId, out T fakedValue)
        => _values.TryGetValue(netId, out fakedValue);

    /// <summary>
    /// Attempts to retrieve a fake value for a specific player.
    /// </summary>
    /// <param name="hub">The target player.</param>
    /// <param name="fakedValue">The retrieved fake value.</param>
    /// <returns><see langword="true"/> if a faked value was found</returns>
    public bool TryGetValue(ReferenceHub hub, out T fakedValue)
        => _values.TryGetValue(hub.netId, out fakedValue);

    /// <summary>
    /// Attempts to retrieve a fake value for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="fakedValue">The retrieved fake value.</param>
    /// <returns><see langword="true"/> if a faked value was found</returns>
    public bool TryGetValue(ExPlayer player, out T fakedValue)
        => _values.TryGetValue(player.NetworkId, out fakedValue);

    /// <summary>
    /// Sets the faked value for a specific network ID.
    /// </summary>
    /// <param name="netId">The target network ID.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(uint netId, T value)
        => _values[netId] = value;

    /// <summary>
    /// Sets the faked value for a specific player.
    /// </summary>
    /// <param name="hub">The target player.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(ReferenceHub hub, T value)
        => _values[hub.netId] = value;

    /// <summary>
    /// Sets the faked value for a specific player.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="value">The value to set.</param>
    public void SetValue(ExPlayer player, T value)
        => _values[player.NetworkId] = value;

    /// <summary>
    /// Removes the faked value of a specific network ID.
    /// </summary>
    /// <param name="netId">The network ID.</param>
    /// <returns><see langword="true"/> if a value was removed</returns>
    public bool RemoveValue(uint netId)
        => _values.Remove(netId);

    /// <summary>
    /// Removes the faked value of a specific player.
    /// </summary>
    /// <param name="hub">The player.</param>
    /// <returns><see langword="true"/> if a value was removed</returns>
    public bool RemoveValue(ReferenceHub hub)
        => _values.Remove(hub.netId);

    /// <summary>
    /// Removes the faked value of a specific player.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns><see langword="true"/> if a value was removed</returns>
    public bool RemoveValue(ExPlayer player)
        => _values.Remove(player.NetworkId);

    /// <summary>
    /// Clears all faked values and / or clears the global value.
    /// </summary>
    /// <param name="clearValues">Whether or not to clear the dictionary of faked values.</param>
    /// <param name="resetGlobalValue">Whether or not to clear the globally faked value.</param>
    public void ClearValues(bool clearValues = true, bool resetGlobalValue = true)
    {
        if (clearValues)
            _values.Clear();

        if (resetGlobalValue)
            ResetGlobalValue();
    }
}