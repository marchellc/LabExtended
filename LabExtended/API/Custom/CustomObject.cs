using YamlDotNet.Serialization;

namespace LabExtended.API.Custom
{
    /// <summary>
    /// Base class for custom objects.
    /// </summary>
    public abstract class CustomObject<T> where T : class
    {
        private static Dictionary<string, T> registered = new();

        /// <summary>
        /// Gets a read-only dictionary of all registered custom objects.
        /// </summary>
        public static IReadOnlyDictionary<string, T> RegisteredObjects => registered;

        /// <summary>
        /// Retrieves the registered object associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the registered object to retrieve. Cannot be null or empty.</param>
        /// <returns>The object of type T that is registered with the specified identifier.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="id"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no object is registered with the specified <paramref name="id"/>.</exception>
        public static T Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            if (!registered.TryGetValue(id, out var customObject))
                throw new KeyNotFoundException($"No custom object with ID '{id}' is registered.");

            return customObject;
        }

        /// <summary>
        /// Retrieves the first registered object that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">A delegate that defines the conditions of the object to search for. The predicate is applied to each
        /// registered object until a match is found.</param>
        /// <returns>The first registered object that satisfies the specified predicate.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="predicate"/> is <see langword="null"/>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if no registered object matches the specified predicate.</exception>
        public static T Get(Predicate<T> predicate)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            foreach (var obj in registered.Values)
            {
                if (predicate(obj))
                {
                    return obj;
                }
            }

            throw new KeyNotFoundException("No custom object matching the given predicate is registered.");
        }

        /// <summary>
        /// Attempts to retrieve a registered object of type T associated with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the object to retrieve. Cannot be null or empty.</param>
        /// <param name="customObject">When this method returns, contains the object associated with the specified identifier, if found; otherwise,
        /// the default value for type T.</param>
        /// <returns>true if an object with the specified identifier is found; otherwise, false.</returns>
        public static bool TryGet(string id, out T customObject)
        {
            customObject = null!;

            if (string.IsNullOrEmpty(id))
                return false;

            return registered.TryGetValue(id, out customObject);
        }

        /// <summary>
        /// Attempts to retrieve the first registered object that matches the specified predicate.
        /// </summary>
        /// <param name="predicate">A delegate that defines the conditions of the object to search for. Cannot be null.</param>
        /// <param name="customObject">When this method returns, contains the first object that matches the predicate, if found; otherwise, the
        /// default value for type T.</param>
        /// <returns>true if an object matching the predicate is found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if predicate is null.</exception>
        public static bool TryGet(Predicate<T> predicate, out T customObject)
        {
            if (predicate is null)
                throw new ArgumentNullException(nameof(predicate));

            customObject = null!;

            foreach (var obj in registered.Values)
            {
                if (predicate(obj))
                {
                    customObject = obj;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the ID of the custom object.
        /// </summary>
        [YamlIgnore]
        public abstract string Id { get; }

        /// <summary>
        /// Whether or not the object is registered.
        /// </summary>
        [YamlIgnore]
        public bool IsRegistered { get; private set; }

        /// <summary>
        /// Registers this custom object.
        /// </summary>
        /// <returns>true if the object got registered</returns>
        public bool Register()
        {
            if (registered.ContainsKey(Id))
                return false;

            registered.Add(Id, (T)(object)this);

            IsRegistered = true;

            OnRegistered();
            return true;
        }

        /// <summary>
        /// Unregisters this custom object.
        /// </summary>
        /// <returns>true if the object got unregistered</returns>
        public bool Unregister()
        {
            if (!registered.Remove(Id))
                return false;

            IsRegistered = false;

            OnUnregistered();
            return true;
        }

        /// <summary>
        /// Gets called after being registered.
        /// </summary>
        public virtual void OnRegistered()
        {

        }

        /// <summary>
        /// Gets called after being unregistered.
        /// </summary>
        public virtual void OnUnregistered()
        {

        }
    }
}