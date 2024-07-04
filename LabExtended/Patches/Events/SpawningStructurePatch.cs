﻿using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using LabExtended.Core.Hooking;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(StructureDistributor), nameof(StructureDistributor.SpawnStructure))]
    public static class SpawningStructurePatch
    {
        public static bool Prefix(StructureDistributor __instance, SpawnableStructure structure, Transform tr, string doorName)
        {
            DoorVariant triggerDoor = null;

            if (!string.IsNullOrWhiteSpace(doorName) && DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var triggerNameTag))
                triggerDoor = triggerNameTag.TargetDoor;

            var spawningArgs = new SpawningStructureArgs(structure, tr, triggerDoor);

            if (!HookRunner.RunCancellable(spawningArgs, true))
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