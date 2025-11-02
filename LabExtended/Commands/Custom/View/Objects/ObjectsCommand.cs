using LabExtended.Commands.Attributes;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;

using Mirror;

using NorthwoodLib.Pools;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace LabExtended.Commands.Custom.View;

public partial class ViewCommand
{
    /// <summary>
    /// Lists all currently spawned network objects, optionally filtered by object type.
    /// </summary>
    /// <remarks>If no objects of the specified type are found, a message is displayed indicating that no such
    /// objects are spawned. When listing all objects, the results are grouped by type and include the count and network
    /// IDs of each group.</remarks>
    /// <param name="objectType">The name of the object's type to filter the results by. Specify "all" to list all spawned objects. The
    /// comparison is case-insensitive.</param>
    [CommandOverload("objects", "Lists all spawned network objects.", null)]
    public void ListObjects(
        [CommandParameter("Type", "Name of the object's type (use \"all\" to show all objects).")] string objectType = "all")
    {
        Ok(x =>
        {
            if (NetworkServer.spawned.Count < 1)
            {
                x.AppendLine("There aren't any spawned objects.");
                return;
            }

            if (objectType != null && !string.Equals(objectType, "all", StringComparison.InvariantCultureIgnoreCase))
            {
                var targetList = ListPool<NetworkBehaviour>.Shared.Rent();

                foreach (var pair in NetworkServer.spawned)
                {
                    if (!pair.Value.gameObject.TryFindComponent<NetworkBehaviour>(out var networkBehaviour))
                        continue;

                    var type = networkBehaviour.GetType();

                    if (string.Equals(objectType, type.FullName, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(objectType, type.Name, StringComparison.InvariantCultureIgnoreCase))
                        targetList.Add(networkBehaviour);
                }

                if (targetList.Count < 1)
                {
                    ListPool<NetworkBehaviour>.Shared.Return(targetList);

                    x.AppendLine($"No objects of type \"{objectType}\" are spawned.");
                    return;
                }

                foreach (var behaviour in targetList)
                    x.AppendLine(
                        $"[{behaviour.netId}] {behaviour.name} (distance: {Sender.Position.DistanceTo(behaviour.transform)}m)");

                ListPool<NetworkBehaviour>.Shared.Return(targetList);
                return;
            }

            var groupedObjects = DictionaryPool<Type, List<uint>>.Shared.Rent();

            foreach (var pair in NetworkServer.spawned)
            {
                if (!pair.Value.gameObject.TryFindComponent<NetworkBehaviour>(out var networkBehaviour))
                    continue;

                if (!groupedObjects.TryGetValue(networkBehaviour.GetType(), out var list))
                    groupedObjects.Add(networkBehaviour.GetType(), list = ListPool<uint>.Shared.Rent());

                list.Add(pair.Key);
            }

            foreach (var list in groupedObjects)
            {
                x.AppendLine(
                    $"[{list.Key.Name}] {list.Value.Count} object(s) [{string.Join(", ", list.Value).SubstringPostfix(24, " ...")}]");
                
                ListPool<uint>.Shared.Return(list.Value);
            }
            
            DictionaryPool<Type, List<uint>>.Shared.Return(groupedObjects);
        });
    }
}