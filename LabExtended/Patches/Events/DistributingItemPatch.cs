using Common.Pooling.Pools;

using Interactables.Interobjects.DoorUtils;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.Core.Hooking;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.Patches.Events
{
    public static class DistributingItemPatch
    {
        public static bool Prefix(ItemDistributor __instance)
        {
            while (ItemSpawnpoint.RandomInstances.Remove(null)) { }
            while (ItemSpawnpoint.AutospawnInstances.Remove(null)) { }

            foreach (var item in __instance.Settings.SpawnableItems)
                SpawnItem(__instance, item);

            foreach (var spawnPoint in ItemSpawnpoint.AutospawnInstances)
                SpawnItem(__instance, spawnPoint);

            return false;
        }

        private static void SpawnItem(ItemDistributor distributor, SpawnableItem spawnableItem)
        {
            var amount = UnityEngine.Random.Range(spawnableItem.MinimalAmount, spawnableItem.MaxAmount);
            var list = ListPool<ItemSpawnpoint>.Shared.Rent();

            foreach (var randomInstance in ItemSpawnpoint.RandomInstances)
            {
                if (spawnableItem.RoomNames.Contains(randomInstance.RoomName) && randomInstance.CanSpawn(spawnableItem.PossibleSpawns))
                    list.Add(randomInstance);
            }

            if (spawnableItem.MultiplyBySpawnpointsNumber)
                amount *= list.Count;

            var distributingPickupsArgs = new DistributingPickupsArgs(spawnableItem, (int)amount, list);

            if (!HookRunner.RunCancellable(distributingPickupsArgs, true))
            {
                ListPool<ItemSpawnpoint>.Shared.Return(list);
                return;
            }

            amount = distributingPickupsArgs.Amount;

            for (int i = 0; i < amount; i++)
            {
                if (list.Count < 1)
                    break;

                var type = spawnableItem.PossibleSpawns[UnityEngine.Random.Range(0, spawnableItem.PossibleSpawns.Length)];

                if (type is ItemType.None)
                    continue;

                var spawnpointIndex = UnityEngine.Random.Range(0, list.Count);
                var distributingArgs = new DistributingPickupArgs(list[spawnpointIndex], type, list[spawnpointIndex].Occupy());

                if (!HookRunner.RunCancellable(distributingArgs, true) || !EventManager.ExecuteEvent(new ItemSpawnedEvent(distributingArgs.Type, distributingArgs.TargetTransform.position)))
                {
                    list[spawnpointIndex]._uses--;
                    continue;
                }

                CreatePickup(distributor, distributingArgs.SpawnPoint, distributingArgs.Type, distributingArgs.TargetTransform, distributingArgs.TargetPosition, distributingArgs.SpawnPoint.TriggerDoorName);

                if (!distributingArgs.SpawnPoint.CanSpawn(distributingArgs.Type))
                    list.RemoveAt(spawnpointIndex);
            }

            ListPool<ItemSpawnpoint>.Shared.Return(list);
        }

        private static void SpawnItem(ItemDistributor distributor, ItemSpawnpoint itemSpawnpoint)
        {
            var transform = itemSpawnpoint.Occupy();
            var distributingArgs = new DistributingPickupArgs(itemSpawnpoint, itemSpawnpoint.AutospawnItem, transform);

            if (!HookRunner.RunCancellable(distributingArgs, true) || !EventManager.ExecuteEvent(new ItemSpawnedEvent(itemSpawnpoint.AutospawnItem, transform.position)))
            {
                itemSpawnpoint._uses--;
                return;
            }

            CreatePickup(distributor, itemSpawnpoint, distributingArgs.Type, distributingArgs.TargetTransform, distributingArgs.TargetPosition, itemSpawnpoint.TriggerDoorName);
        }

        private static void CreatePickup(ItemDistributor distributor, ItemSpawnpoint spawnpoint, ItemType type, Transform transform, Vector3? customPos, string doorName)
        {
            if (!InventoryItemLoader.TryGetItem<ItemBase>(type, out var prefab))
                return;

            var position = Vector3.zero;
            var rotation = Quaternion.identity;

            if (transform != null)
            {
                position = transform.position;
                rotation = transform.rotation;
            }

            if (customPos.HasValue)
                position = customPos.Value;

            var pickup = UnityEngine.Object.Instantiate(prefab.PickupDropModel, position, rotation);

            pickup.Info.ItemId = type;
            pickup.Info.WeightKg = prefab.Weight;

            if (transform != null)
                pickup.transform.SetParent(transform);

            if (pickup is IPickupDistributorTrigger pickupDistributorTrigger)
                pickupDistributorTrigger.OnDistributed();

            if (string.IsNullOrWhiteSpace(doorName) || !DoorNametagExtension.NamedDoors.TryGetValue(doorName, out var door))
            {
                ItemDistributor.SpawnPickup(pickup);
                HookRunner.RunEvent(new DistributedPickupArgs(spawnpoint, pickup));
            }
            else
                distributor.RegisterUnspawnedObject(door.TargetDoor, pickup.gameObject);
        }
    }
}