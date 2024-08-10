using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;

using MapGeneration;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(DoorSpawnpoint), nameof(DoorSpawnpoint.SetupAllDoors))]
    public static class DoorPrefabPatch
    {
        public static bool Prefix()
        {
            Prefabs._prefabDoors.Clear();

            foreach (var spawnpoint in DoorSpawnpoint.AllInstances)
            {
                if (spawnpoint.TargetPrefab is null)
                    continue;

                var prefabType = spawnpoint.TargetPrefab.name switch
                {
                    "LCZ BreakableDoor" => PrefabType.LightContainmentZoneDoor,
                    "HCZ BreakableDoor" => PrefabType.HeavyContainmentZoneDoor,
                    "EZ BreakableDoor" => PrefabType.EntranceZoneDoor,

                    _ => throw new ArgumentException($"Unknown door prefab: {spawnpoint.TargetPrefab.name}")
                };

                if (!Prefabs._prefabDoors.ContainsKey(prefabType))
                    Prefabs._prefabDoors.Add(prefabType, spawnpoint.TargetPrefab);

                if (!Prefabs._prefabObjects.ContainsKey(prefabType))
                    Prefabs._prefabObjects.Add(prefabType, spawnpoint.TargetPrefab.gameObject);
            }

            return true;
        }
    }
}