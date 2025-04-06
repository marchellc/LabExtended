using LabExtended.API.Prefabs;
using LabExtended.Commands.Attributes;

using Mirror;

using UnityEngine;

namespace LabExtended.Commands.Custom.Spawn;

public partial class SpawnCommand
{
    [CommandOverload("prefab", "Spawns a prefab")]
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