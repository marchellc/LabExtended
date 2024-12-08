using LabExtended.API.Prefabs;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using Mirror;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Spawn.Prefab
{
    public class PrefabCommand : CustomCommand
    {
        public override string Command => "prefab";
        public override string Description => "Prefab spawn command.";

        public override ArgumentDefinition[] BuildArgs()
        {
            return GetArgs(x =>
            {
                x.WithArg<string>("Name", "Name of the prefab.");

                x.WithOptional<Vector3>("Scale", "Scale of the spawned prefab.", Vector3.zero);
                x.WithOptional<Vector3>("Position", "Position of the prefab", Vector3.zero);
            });
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection collection)
        {
            base.OnCommand(sender, ctx, collection);

            var prefabName = collection.GetString("Name");
            var prefabScale = collection.Get<Vector3>("Scale");
            var prefabPosition = collection.Get<Vector3>("Position");

            if (prefabPosition == Vector3.zero)
                prefabPosition = sender.Position;

            if (!PrefabList.AllPrefabs.TryGetValue(prefabName, out var prefab))
            {
                ctx.RespondFail($"Unknown prefab: {prefabName}");
                return;
            }

            var spawnedPrefab = prefab.Spawn<NetworkIdentity>(prefab =>
            {
                if (prefabScale != Vector3.zero)
                    prefab.transform.localScale = prefabScale;

                prefab.transform.position = prefabPosition;
                prefab.transform.rotation = sender.Rotation;
            });

            ctx.RespondOk($"Spawned prefab '{spawnedPrefab.name}' with network ID '{spawnedPrefab.netId}'");
        }
    }
}