using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.Prefabs;

/// <summary>
/// Represents a spawnable in-game prefab.
/// </summary>
public class PrefabDefinition
{
    /// <summary>
    /// Gets the prefab's name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the prefab's instance.
    /// </summary>
    public GameObject? GameObject
    {
        get
        {
            if (field is null)
            {
                if (string.IsNullOrWhiteSpace(Name))
                    throw new Exception("Empty prefab name");

                if (!NetworkClient.prefabs.TryGetFirst(x => x.Value.name.Trim() == Name, out var prefabPair))
                    throw new Exception($"Prefab '{Name}' could not be found");

                field = prefabPair.Value;
            }

            return field;
        }
    }

    /// <summary>
    /// Spawns a new instance from this prefab.
    /// </summary>
    /// <param name="prefabSetup"></param>
    /// <returns></returns>
    public GameObject Spawn(Action<GameObject> prefabSetup = null)
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
    public T Spawn<T>(Action<T> prefabSetup = null) where T : Component
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
    public GameObject CreateInstance()
        => UnityEngine.Object.Instantiate(GameObject);

    /// <summary>
    /// Spawns an instantiated prefab instance.
    /// </summary>
    /// <param name="identity"></param>
    public void SpawnInstance(NetworkIdentity identity)
        => NetworkServer.Spawn(identity.gameObject);

    /// <summary>
    /// This method is used only for custom definitions that require a different spawn method.
    /// </summary>
    /// <param name="identity">The network identity instance.</param>
    /// <param name="position">The position to spawn at.</param>
    /// <param name="scale">The scale to spawn at.</param>
    /// <param name="rotation">The rotation to spawn with.</param>
    public void SetupInstance(NetworkIdentity identity, Vector3 position, Vector3 scale, Quaternion rotation)
    {
    }

    /// <summary>
    /// Creates a new <see cref="PrefabDefinition"/> instance.
    /// </summary>
    /// <param name="name">Name of the prefab object.</param>
    public PrefabDefinition(string name) => Name = name;
}
