using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Extensions;

using LabExtended.Events;
using LabExtended.Events.Map;

using MapGeneration.Distributors;

using Object = UnityEngine.Object;

namespace LabExtended.Patches.Events;

/// <summary>
/// Provides the <see cref="ExMapEvents.LockerFillingChamber"/> event.
/// </summary>
public static class LockerSpawningPickupPatch
{
    [HarmonyPatch(typeof(LockerChamber), nameof(LockerChamber.SpawnItem))]
    private static bool Prefix(LockerChamber __instance, ItemType id, int amount)
    {
        if (id is ItemType.None || !id.TryGetItemPrefab(out var prefab))
            return false;
        
        var fillingArgs =
            new LockerFillingChamberEventArgs(ExMap.Lockers.First(x => x.Chambers.Contains(__instance)),
                __instance, __instance.Spawnpoint, amount, id);

        if (!ExMapEvents.OnLockerFillingChamber(fillingArgs))
            return false;

        id = fillingArgs.Type;
        amount = fillingArgs.Amount;
        
        for (var i = 0; i < amount; i++)
        {
            __instance.GetSpawnpoint(id, i , out var worldPosition, out var worldRotation, out var parent);
                
            var pickup = Object.Instantiate(prefab.PickupDropModel, worldPosition, worldRotation);
            
            pickup.transform.SetParent(parent);
            pickup.NetworkInfo = new(id, prefab.Weight, 0, true);
            
            __instance.Content.Add(pickup);
            
            (pickup as IPickupDistributorTrigger)?.OnDistributed();

            if (pickup.TryGetRigidbody(out var rigidbody))
            {
                rigidbody.isKinematic = true;
                rigidbody.transform.ResetLocalPose();
            }
            
            if (__instance.SpawnOnFirstChamberOpening)
                __instance.ToBeSpawned.Add(pickup);
            else
                ItemDistributor.SpawnPickup(pickup);
        }
        
        return false;
    }
}