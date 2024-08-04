using LabExtended.API.Modules;

namespace LabExtended.API.CustomModules
{
    /// <summary>
    /// A transient module used for data storage between sessions.
    /// </summary>
    public class PlayerStorageModule : TransientModule
    {
        private readonly Dictionary<string, object> _storage = new Dictionary<string, object>();

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
        public object Get(string name, object defaultValue = null)
            => _storage.TryGetValue(name, out var value) ? value : defaultValue;

        /// <summary>
        /// Gets the value saved under the <paramref name="name"/>.
        /// </summary>
        /// <typeparam name="T">Type of the value to retrieve.</typeparam>
        /// <param name="name">The name the value was saved under.</param>
        /// <param name="defaultValue">The value to return if the specified name was not found.</param>
        /// <returns>The saved value if found, otherwise <paramref name="defaultValue"/>.</returns>
        public T Get<T>(string name, T defaultValue = default)
        {
            if (_storage.TryGetValue(name, out var cachedValue) && cachedValue != null && cachedValue is T castValue)
                return castValue;

            return defaultValue;
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
    }
}