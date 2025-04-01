using LabExtended.API.Prefabs;
using LabExtended.Commands.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.List;

public partial class ListCommand
{
    [CommandOverload("prefabs", "Lists prefab names.")]
    public void PrefabsOverload()
    {
        Ok(x =>
        {
            x.AppendLine($"{PrefabList.AllPrefabs.Count} prefabs");

            foreach (var pair in PrefabList.AllPrefabs)
            {
                x.AppendLine($"[{pair.Key}] {pair.Value.GameObject.name}");
            }
        });
    }
}