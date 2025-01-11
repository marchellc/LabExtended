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
        public virtual string Name { get; }
        
        /// <summary>
        /// Whether this is a custom prefab definition.
        /// </summary>
        public virtual bool IsCustom { get; }

        /// <summary>
        /// Gets the prefab's instance.
        /// </summary>
        public virtual GameObject GameObject
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
        /// Spawns a new instance from this prefab.
        /// </summary>
        /// <param name="prefabSetup"></param>
        /// <returns></returns>
        public virtual GameObject Spawn(Action<GameObject> prefabSetup = null)
        {
            var instance = CreateInstance();

            prefabSetup.InvokeSafe(instance);

            SpawnInstance(instance.GetComponent<NetworkIdentity>());
            return instance;
        }

        /// <summary>
        /// Spawns a new instance from this prefab.
        /// </summary>
        /// <typeparam name="T">Component type.</typeparam>
        /// <param name="prefabSetup"></param>
        /// <returns></returns>
        public virtual T Spawn<T>(Action<T> prefabSetup = null) where T : Component
        {
            var instance = CreateInstance();
            var component = default(T);

            if ((component = instance.GetComponent<T>()) is null
                && (component = instance.GetComponentInChildren<T>()) is null
                && (component = instance.GetComponentInParent<T>()) is null)
                throw new Exception($"Component of type '{typeof(T).FullName}' could not be found in prefab '{Name}'");

            prefabSetup.InvokeSafe(component);

            SpawnInstance(instance.GetComponent<NetworkIdentity>());
            return component;
        }

        /// <summary>
        /// Creates a new instance from this prefab.
        /// </summary>
        public virtual GameObject CreateInstance()
            => UnityEngine.Object.Instantiate(GameObject);
        
        /// <summary>
        /// Spawns an instantiated prefab instance.
        /// </summary>
        /// <param name="identity"></param>
        public virtual void SpawnInstance(NetworkIdentity identity)
            => NetworkServer.Spawn(identity.gameObject);

        /// <summary>
        /// This method is used only for custom definitions that require a different spawn method.
        /// </summary>
        /// <param name="identity">The network identity instance.</param>
        /// <param name="position">The position to spawn at.</param>
        /// <param name="scale">The scale to spawn at.</param>
        /// <param name="rotation">The rotation to spawn with.</param>
        public virtual void SetupInstance(NetworkIdentity identity, Vector3 position, Vector3 scale, Quaternion rotation) { }

        public PrefabDefinition(string name) => Name = name;
        public PrefabDefinition() { }
    }
}
