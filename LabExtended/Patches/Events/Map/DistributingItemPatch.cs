using HarmonyLib;

using Interactables.Interobjects.DoorUtils;

using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

namespace LabExtended.Patches.Events.Map
{
    public static class DistributingItemPatch
    {
        [EventPatch(typeof(DistributedPickupEventArgs))]
        [EventPatch(typeof(DistributingPickupEventArgs))]
        [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.ServerRegisterPickup))]
        public static bool Prefix(ItemDistributor __instance, ItemPickupBase pickup, string triggerDoor)
        {
            var spawningArgs = new ItemSpawningEventArgs(pickup.ItemId.TypeId);
            
            ServerEvents.OnItemSpawning(spawningArgs);

            if (!spawningArgs.IsAllowed)
                return false;

            var distributingArgs = new DistributingPickupEventArgs(pickup);

            if (!ExMapEvents.OnDistributingPickup(distributingArgs))
                return false;
            
            (pickup as IPickupDistributorTrigger)?.OnDistributed();

            if (string.IsNullOrEmpty(triggerDoor) ||
                !DoorNametagExtension.NamedDoors.TryGetValue(triggerDoor, out var doorNametag))
            {
                ItemDistributor.SpawnPickup(pickup);
                ServerEvents.OnItemSpawned(new(pickup));
                ExMapEvents.OnDistributedPickup(new(pickup));
            }
            else
            {
                __instance.RegisterUnspawnedObject(doorNametag.TargetDoor, pickup.gameObject);
            }

            return false;
        }
    }
}