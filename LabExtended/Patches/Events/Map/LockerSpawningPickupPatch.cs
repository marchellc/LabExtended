using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using Object = UnityEngine.Object;

namespace LabExtended.Patches.Events;

/// <summary>
/// Provides the <see cref="ExMapEvents.LockerSpawningPickup"/> and <see cref="ExMapEvents.LockerSpawnedPickup"/> events.
/// </summary>
public static class LockerSpawningPickupPatch
{
    [HarmonyPatch(typeof(LockerChamber), nameof(LockerChamber.SpawnItem))]
    private static bool Prefix(LockerChamber __instance, ItemType id, int amount)
    {
        if (!ExMap.ChamberToLocker.TryGetValue(__instance, out var locker))
        {
            if (__instance.gameObject.TryFindComponent(out locker))
            {
                ExMap.RegisterChamber(__instance, locker);
            }
            else
            {
                ApiLog.Warn("LockerSpawningPickupPatch", $"Could not find parent locker of chamber &3{__instance.GetInstanceID()}&r");
                return false;
            }
        }

        for (var i = 0; i < amount; i++)
        {
            __instance.GetSpawnpoint(id, i , out var worldPosition, out var worldRotation, out var parent);

            var spawningArgs = new LockerSpawningPickupEventArgs(locker, __instance, id, worldPosition, worldRotation);
            
            if (!ExMapEvents.OnLockerSpawningPickup(spawningArgs))
                continue;
            
            if (spawningArgs.Type is ItemType.None || !spawningArgs.Type.TryGetItemPrefab(out var prefab))
                continue;
            
            var pickup = Object.Instantiate(prefab.PickupDropModel, worldPosition, worldRotation);
            
            pickup.transform.SetParent(parent);
            pickup.NetworkInfo = new(id, prefab.Weight, 0, true);
            
            __instance.Content.Add(pickup);
            
            (pickup as IPickupDistributorTrigger)?.OnDistributed();

            if (pickup.TryGetRigidbody(out var rigidbody))
            {
                rigidbody.isKinematic = true;
                rigidbody.transform.ResetLocalPose();

                SpawnablesDistributorBase.BodiesToUnfreeze.Add(rigidbody);
            }

            if (__instance.SpawnOnFirstChamberOpening)
            {
                __instance.ToBeSpawned.Add(pickup);
            }
            else
            {
                ItemDistributor.SpawnPickup(pickup);
                
                ExMapEvents.OnLockerSpawnedPickup(new(locker, __instance, pickup));
            }
        }
        
        return false;
    }

    [HarmonyPatch(typeof(LockerChamber), nameof(LockerChamber.OnFirstTimeOpen))]
    private static bool ChamberFirstTimeOpenPrefix(LockerChamber __instance)
    {
        __instance.Content.ForEach(pickup =>
        {
            if (pickup != null)
            {
                var info = pickup.Info;

                info.Locked = false;

                pickup.NetworkInfo = info;
            }
        });

        if (!__instance.SpawnOnFirstChamberOpening)
            return false;

        if (!ExMap.ChamberToLocker.TryGetValue(__instance, out var locker))
        {
            if (__instance.gameObject.TryFindComponent(out locker))
            {
                ExMap.RegisterChamber(__instance, locker);
            }
            else
            {
                ApiLog.Warn("LockerSpawningPickupPatch", $"Could not find parent locker of chamber &3{__instance.GetInstanceID()}&r");
                return false;
            }
        }

        __instance.ToBeSpawned.ForEach(pickup =>
        {
            if (pickup != null)
            {
                ItemDistributor.SpawnPickup(pickup);
                
                ExMapEvents.OnLockerSpawnedPickup(new(locker, __instance, pickup));
            }
        });

        return false;
    }
}