using LabExtended.Core.Pooling.Pools;

namespace LabExtended.API
{
    /// <summary>
    /// A transient module used for data storage between sessions.
    /// </summary>
    public class PlayerStorage : IDisposable
    {
        internal static readonly Dictionary<string, PlayerStorage?> _persistentStorage = new Dictionary<string, PlayerStorage?>();

        private Dictionary<string, object>? _storage;
        
        /// <summary>
        /// Whether or not this storage is persistent.
        /// </summary>
        public bool IsPersistent { get; }

        /// <summary>
        /// Whether or not the player that owns this storage is online.
        /// </summary>
        public bool IsJoined => Player != null && Player;
        
        /// <summary>
        /// Whether or not this storage is empty.
        /// </summary>
        public bool IsEmpty => _storage.Count == 0;
        
        /// <summary>
        /// Amount of items currently in this storage.
        /// </summary>
        public int Count => _storage.Count;

        /// <summary>
        /// Gets the amount of times this storage was re-added to a specific player.
        /// </summary>
        public int Lifes { get; internal set; } = 0;
        
        /// <summary>
        /// Gets the player that this storage belongs to.
        /// </summary>
        public ExPlayer? Player { get; internal set; }
        
        /// <summary>
        /// Gets the time when the player left. (only for a permanent storage)
        /// </summary>
        public DateTime LeaveTime { get; internal set; }
        
        /// <summary>
        /// Gets the time when the player joined.
        /// </summary>
        public DateTime JoinTime { get; internal set; }

        /// <summary>
        /// Gets or sets an item in storage.
        /// </summary>
        /// <param name="key">The item's key.</param>
        public object this[string key]
        {
            get => _storage[key];
            set => _storage[key] = value;
        }

        /// <summary>
        /// Creates a new <see cref="PlayerStorage"/> instance.
        /// </summary>
        /// <param name="isPersistent">Whether or not this storage is going to be persistent.</param>
        /// <param name="player">The player that this storage targets.</param>
        public PlayerStorage(bool isPersistent, ExPlayer? player = null)
        {
            Player = player;
            IsPersistent = isPersistent;

            _storage = DictionaryPool<string, object>.Shared.Rent();
        }

        /// <summary>
        /// Tries to retrieve the <paramref name="value"/> saved under the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>Whether or not the value was succesfully retrieved.</returns>
        public bool TryGet(string name, out object value)
            => _storage.TryGetValue(name, out value);

        /// <summary>
        /// Tries to retrieve the <paramref name="value"/> saved under the <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="value">The retrieved value.</param>
        /// <returns>Whether or not the value was succesfully retrieved.</returns>
        public bool TryGet<T>(string name, out T value)
        {
            if (_storage.TryGetValue(name, out var cachedValue) && cachedValue != null && cachedValue is T castValue)
            {
                value = castValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value saved under the <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="defaultValue">The value to return if the specified name was not found.</param>
        /// <returns>The saved value if found, otherwise <paramref name="defaultValue"/>.</returns>
        public object Get(string name, object? defaultValue = null)
            => _storage!.TryGetValue(name, out var value) ? value : defaultValue!;

        /// <summary>
        /// Gets the value saved under the <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve.</typeparam>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="defaultValue">The value to return if the specified name was not found.</param>
        /// <returns>The saved value if found, otherwise <paramref name="defaultValue"/>.</returns>
        public T Get<T>(string name, T? defaultValue = default)
        {
            if (_storage!.TryGetValue(name, out var cachedValue) && cachedValue != null && cachedValue is T castValue)
                return castValue;

            return defaultValue!;
        }

        /// <summary>
        /// Gets the value saved under the <paramref name="name"/> (or creates a new one).
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve.</typeparam>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="factory">The method used to construct a new item instance.</param>
        /// <returns>The saved value if found, otherwise <paramref name="defaultValue"/>.</returns>
        public T GetOrAdd<T>(string name, Func<T> factory)
        {
            if (_storage.TryGetValue(name, out var cachedValue) && cachedValue != null && cachedValue is T castValue)
                return castValue;

            var value = factory();

            _storage[name] = value;
            return value;
        }

        /// <summary>
        /// Saves a <paramref name="value"/> under a specific <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to save the <paramref name="value"/> under.</param>
        /// <param name="value">The value to save.</param>
        /// <param name="onlyIfNew">Only add if this name has not been added yet. Setting it to false will result in overriding the currently saved value (default behaviour).</param>
        public void Set(string name, object value, bool onlyIfNew = false)
        {
            if (onlyIfNew && _storage.ContainsKey(name))
                return;

            _storage[name] = value;
        }

        /// <summary>
        /// Removes a saved value.
        /// </summary>
        /// <param name="key">The name of the value to remove.</param>
        /// <returns>Whether or not the value was succesfully removed.</returns>
        public bool Remove(string key)
            => _storage.Remove(key);

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_storage != null)
            {
                DictionaryPool<string, object>.Shared.Return(_storage);

                _storage = null;
            }
        }
    }
}