using LabExtended.API.Prefabs;
using LabExtended.Commands.Attributes;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    /// <summary>
    /// Lists the names of all available prefabs to the command output.
    /// </summary>
    /// <remarks>This method is intended to be used as a command overload for displaying all registered prefab
    /// names. The output includes the total count of prefabs followed by each prefab's identifier and name.</remarks>
    [CommandOverload("prefabs", "Lists prefab names.", null)]
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