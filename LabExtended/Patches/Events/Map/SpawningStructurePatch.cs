using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    public static class SpawningStructurePatch
    {
        [EventPatch(typeof(SpawningStructureEventArgs))]
        [HarmonyPatch(typeof(StructureDistributor), nameof(StructureDistributor.SpawnStructure))]
        public static bool Prefix(StructureDistributor __instance, SpawnableStructure structure, Transform tr, string doorName)
        {
            DoorVariant triggerDoor = null;

            if (!string.IsNullOrWhiteSpace(doorName) && DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var triggerNameTag))
                triggerDoor = triggerNameTag.TargetDoor;

            var spawningArgs = new SpawningStructureEventArgs(structure, tr, triggerDoor != null ? Door.Get(triggerDoor) : null);

            if (!ExMapEvents.OnSpawningStructure(spawningArgs))
                return false;

            var obj = UnityEngine.Object.Instantiate(structure, tr.position, tr.rotation);

            obj.transform.SetParent(tr);

            if (spawningArgs.TriggerDoor is null)
                __instance.SpawnObject(obj.gameObject);
            else
                __instance.RegisterUnspawnedObject(triggerDoor, obj.gameObject);

            return false;
        }
    }
}