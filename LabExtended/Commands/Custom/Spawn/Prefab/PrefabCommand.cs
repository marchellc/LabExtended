using LabExtended.API.Prefabs;
using LabExtended.Commands.Attributes;

using Mirror;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    /// <summary>
    /// Spawns a prefab with the specified name, size, position, and rotation.
    /// </summary>
    /// <remarks>If a prefab with the specified name cannot be found, the operation fails and no object is
    /// spawned. The spawned object's name and network ID are reported upon success.</remarks>
    /// <param name="prefabName">The name of the prefab to spawn. The search is case-insensitive.</param>
    /// <param name="size">The size to apply to the spawned prefab. If not specified, the default size of one is used.</param>
    /// <param name="position">The world position at which to spawn the prefab. If not specified, the sender's current position is used.</param>
    /// <param name="rotation">The rotation to apply to the spawned prefab. If not specified, the sender's current rotation is used.</param>
    [CommandOverload("prefab", "Spawns a prefab", null)]
    public void PrefabOverload(
        [CommandParameter("Name", "Name of the prefab")] string prefabName, 
        [CommandParameter("Size", "Size of the prefab (defaults to one)")] Vector3? size = null, 
        [CommandParameter("Position", "Spawn position (defaults to your position)")] Vector3? position = null, 
        [CommandParameter("Rotation", "Spawn rotation (defaults to your rotation)")] Quaternion? rotation = null)
    {
        size ??= Vector3.one;
        
        position ??= Sender.Position;
        rotation ??= Sender.Rotation;

        var targetPrefab = default(PrefabDefinition);

        foreach (var pair in PrefabList.AllPrefabs)
        {
            if (string.Equals(pair.Key, prefabName, StringComparison.InvariantCultureIgnoreCase))
            {
                targetPrefab = pair.Value;
                break;
            }
        }
        
        if (targetPrefab is null)
        {
            Fail($"Could not find prefab \"{prefabName}\"");
            return;
        }

        var instance = targetPrefab.Spawn<NetworkIdentity>(obj =>
        {
            obj.transform.position = position.Value;
            obj.transform.rotation = rotation.Value;

            obj.transform.localScale = size.Value;
        });
        
        Ok($"Spawned object \"{instance.name}\" (ID: {instance.netId})");
    }
}