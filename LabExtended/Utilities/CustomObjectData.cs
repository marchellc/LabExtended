using LabExtended.Core.Pooling.Pools;

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Utilities;

/// <summary>
/// Provides the ability to save custom properties for any object.
/// </summary>
public static class CustomObjectData
{
    private static int clock = 0;
    
    /// <summary>
    /// Describes an entry with custom data.
    /// </summary>
    public class CustomObjectEntry : IEquatable<CustomObjectEntry>
    {
        /// <summary>
        /// Gets the ID of the entry.
        /// </summary>
        public readonly int Id;
        
        /// <summary>
        /// Gets the target of the entry.
        /// </summary>
        public readonly object? Target;
        
        /// <summary>
        /// Gets the custom data of the entry.
        /// </summary>
        public object? Custom;
        
        /// <summary>
        /// Gets the custom properties of the entry.
        /// </summary>
        public Dictionary<string, object>? Properties;

        /// <summary>
        /// Creates a new <see cref="CustomObjectEntry"/> instance.
        /// </summary>
        /// <param name="id">The assigned entry ID.</param>
        /// <param name="target">The target of the entry.</param>
        /// <param name="custom">The custom data of the entry.</param>
        /// <param name="properties">The custom properties.</param>
        public CustomObjectEntry(int id, object target, object custom, Dictionary<string, object> properties)
        {
            Id = id;
            Target = target;
            Custom = custom;
            Properties = properties;
        }

        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        public bool Equals(CustomObjectEntry other)
            => other.Id == Id;

        /// <inheritdoc cref="ValueType.Equals(object)"/>
        public override bool Equals(object? obj)
            => obj is CustomObjectEntry other && other.Id == Id;

        /// <inheritdoc cref="ValueType.GetHashCode"/>
        public override int GetHashCode()
            => Id;
    }

    private static volatile List<CustomObjectEntry> entries = new(byte.MaxValue);

    public static T CompareProperty<T>(this object target, string name, T newValue, Func<T?, T, bool> comparer)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var entry = GetOrAddEntry(target);
        var current = entry.Properties.TryGetValue(name, out var value) && value is T result ? result : default;

        if (comparer(current, newValue))
        {
            entry.Properties[name] = newValue;
            return newValue;
        }

        return current;
    }

    public static T GetOrAddProperty<T>(this object target, string name, T defaultValue = default)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var entry = GetOrAddEntry(target);

        if (entry.Properties.TryGetValue(name, out var value) && value is T result)
            return result;

        entry.Properties[name] = defaultValue;
        return defaultValue;
    }

    public static T GetOrAddData<T>(this object target, T defaultValue = default)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        var entry = GetOrAddEntry(target);

        if (entry.Custom is T result)
            return result;

        entry.Custom = defaultValue;
        return defaultValue;
    }

    public static bool RemoveProperty(this object target, string name)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!TryGetEntry(target, out var entry))
            return false;

        return entry.Properties.Remove(name);
    }

    public static bool RemoveData(this object target)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!TryGetEntry(target, out var entry) || entry.Custom is null)
            return false;

        entry.Custom = null;
        return true;
    }

    public static bool TryGetProperty<T>(object target, string name, out T value)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        value = default;

        var entry = GetOrAddEntry(target);

        if (!entry.Properties.TryGetValue(name, out var result))
            return false;

        if (result is not T cast)
            return false;

        value = cast;
        return true;
    }

    public static bool TryGetData<T>(object target, out T data)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        data = default;
        
        var entry = GetOrAddEntry(target);

        if (entry.Custom is not T value)
            return false;

        data = value;
        return true;
    }
    
    public static CustomObjectEntry GetOrAddEntry(object target, object? customData = null)
    {
        if (TryGetEntry(target, out var entry))
            return entry;

        entry = new(clock++, target, customData, DictionaryPool<string, object>.Shared.Rent());
        
        entries.Add(entry);
        return entry;
    }

    public static bool RemoveEntry(object target)
    {
        if (!TryGetEntry(target, out var entry))
            return false;
        
        if (entry.Properties != null)
            DictionaryPool<string, object>.Shared.Return(entry.Properties);

        entry.Properties = null;
        return entries.Remove(entry);
    }
    
    public static bool TryGetEntry(object target, out CustomObjectEntry entry)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));
        
        for (var i = 0; i < entries.Count; i++)
        {
            var current = entries[i];
            
            if (current.Target != target)
                continue;

            entry = current;
            return true;
        }

        entry = default;
        return false;
    }
}