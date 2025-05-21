using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    public static class LockerSpawningPickupPatch
    {
        [HarmonyPatch(typeof(LockerChamber), nameof(LockerChamber.SpawnItem))]
        public static bool Prefix(LockerChamber __instance, ItemType id, int amount)
        {
            if (id is ItemType.None || !id.TryGetItemPrefab(out var prefab))
                return false;

            var spawned = 0;
            var spawnPoint = __instance.Spawnpoint;

            var locker = ExMap.Lockers.FirstOrDefault(locker => locker.Chambers.Contains(__instance));
            var fillingArgs = new LockerFillingChamberEventArgs(locker, __instance, spawnPoint, amount, id);

            if (!ExMapEvents.OnLockerFillingChamber(fillingArgs))
                return false;

            id = fillingArgs.Type;
            amount = fillingArgs.Amount;

            for (int i = 0; i < amount; i++)
            {
                if (__instance._useMultipleSpawnpoints && __instance._spawnpoints.Length > 0)
                {
                    if (spawned > __instance._spawnpoints.Length)
                        spawned = 0;

                    spawnPoint = __instance._spawnpoints[spawned];
                    spawned++;
                }

                var pickup = UnityEngine.Object.Instantiate(prefab.PickupDropModel, spawnPoint.position, spawnPoint.rotation);

                pickup.transform.SetParent(spawnPoint);

                pickup.Info.ItemId = id;
                pickup.Info.WeightKg = prefab.Weight;
                pickup.Info.Locked = true;

                __instance.Content.Add(pickup);

                if (pickup is IPickupDistributorTrigger pickupDistributorTrigger)
                    pickupDistributorTrigger.OnDistributed();

                var rigidbody = pickup.GetRigidbody();

                if (rigidbody != null)
                {
                    rigidbody.isKinematic = true;

                    rigidbody.transform.localPosition = Vector3.zero;
                    rigidbody.transform.localRotation = Quaternion.identity;

                    SpawnablesDistributorBase.BodiesToUnfreeze.Add(rigidbody);
                }

                if (__instance.SpawnOnFirstChamberOpening)
                    __instance.ToBeSpawned.Add(pickup);
                else
                    ItemDistributor.SpawnPickup(pickup);
            }

            return false;
        }
    }
}