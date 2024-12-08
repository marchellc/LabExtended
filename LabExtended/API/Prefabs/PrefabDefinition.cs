using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Prefabs
{
    /// <summary>
    /// Represents a spawnable in-game prefab.
    /// </summary>
    public class PrefabDefinition
    {
        private GameObject _prefabInstance;

        /// <summary>
        /// Gets the prefab's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the prefab's instance.
        /// </summary>
        public GameObject GameObject
        {
            get
            {
                if (_prefabInstance is null)
                {
                    if (string.IsNullOrWhiteSpace(Name))
                        throw new Exception("Empty prefab name");

                    if (!NetworkClient.prefabs.TryGetFirst(x => x.Value.name.Trim() == Name, out var prefabPair))
                        throw new Exception($"Prefab '{Name}' could not be found");

                    _prefabInstance = prefabPair.Value;
                }

                return _prefabInstance;
            }
        }

        /// <summary>
        /// Creates a new instance from this prefab.
        /// </summary>
        public GameObject Instance => UnityEngine.Object.Instantiate(GameObject);

        /// <summary>
        /// Spawns a new instance from this prefab.
        /// </summary>
        /// <param name="prefabSetup"></param>
        /// <returns></returns>
        public GameObject Spawn(Action<GameObject> prefabSetup = null)
        {
            var instance = Instance;

            prefabSetup.InvokeSafe(instance);

            NetworkServer.Spawn(instance);
            return instance;
        }

        /// <summary>
        /// Spawns a new instance from this prefab.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="prefabSetup"></param>
        /// <returns></returns>
        public T Spawn<T>(Action<T> prefabSetup = null) where T : Component
        {
            var instance = Instance;
            var component = default(T);

            if ((component = instance.GetComponent<T>()) is null
                && (component = instance.GetComponentInChildren<T>()) is null
                && (component = instance.GetComponentInParent<T>()) is null)
                throw new Exception($"Component of type '{typeof(T).FullName}' could not be found in prefab '{Name}'");

            prefabSetup.InvokeSafe(component);

            NetworkServer.Spawn(instance);
            return component;
        }

        internal PrefabDefinition(string name) => Name = name;
    }
}
