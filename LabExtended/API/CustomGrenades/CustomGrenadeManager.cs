using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabExtended.API.CustomItems;
using LabExtended.Attributes;
using LabExtended.Events;

using UnityEngine;
using Utils.Networking;

using ThrowableItem = LabApi.Features.Wrappers.ThrowableItem;

namespace LabExtended.API.CustomGrenades;

/// <summary>
/// Manages Custom Grenade items.
/// </summary>
public static class CustomGrenadeManager
{
    private static List<CustomGrenadeInstance> toRemove = new();
    
    /// <summary>
    /// Gets a list of all spawned custom grenades.
    /// </summary>
    public static List<CustomGrenadeInstance> AllGrenades { get; } = new();

    /// <summary>
    /// Spawns a grenade.
    /// </summary>
    /// <param name="customGrenadeInstance">The grenade to spawn.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void SpawnItem(CustomGrenadeInstance customGrenadeInstance)
    {
        if (customGrenadeInstance is null)
            throw new ArgumentNullException(nameof(customGrenadeInstance));

        if (customGrenadeInstance.Item is null)
            throw new Exception("This custom grenade has already been thrown.");

        customGrenadeInstance.OnSpawning();
        
        customGrenadeInstance.Player.Inventory.RemoveItem(customGrenadeInstance.Item);
        customGrenadeInstance.Player.customItems.Remove(customGrenadeInstance.Item);

        var pickup = customGrenadeInstance.Player.Inventory.ThrowItem<ItemPickupBase>(
            customGrenadeInstance.CustomData.PickupType,
            customGrenadeInstance.CustomData.Force, customGrenadeInstance.ItemSerial);
        
        CustomItemManager.PickupItems.Add(pickup, customGrenadeInstance);
        CustomItemManager.InventoryItems.Remove(customGrenadeInstance.Item);
        
        customGrenadeInstance.Item = null;
        customGrenadeInstance.Pickup = pickup;
        
        customGrenadeInstance.RemainingTime = customGrenadeInstance.CustomData.Time;
        customGrenadeInstance.IsSpawned = true;
        
        customGrenadeInstance.OnSpawned();
        
        if (pickup is ThrowableItem)
            new ThrowableNetworkHandler.ThrowableItemAudioMessage(customGrenadeInstance.ItemSerial, ThrowableNetworkHandler.RequestType.ConfirmThrowFullForce).SendToAuthenticated();
    }
    
    private static void OnItemCreated(CustomItemInstance customItemInstance)
    {
        if (customItemInstance is CustomGrenadeInstance customGrenadeInstance)
            AllGrenades.Add(customGrenadeInstance);
    }

    private static void OnItemDestroyed(CustomItemInstance customItemInstance)
    {
        if (customItemInstance is CustomGrenadeInstance customGrenadeInstance)
            AllGrenades.Remove(customGrenadeInstance);
    }

    private static void OnWaiting()
    {
        AllGrenades.Clear();
        toRemove.Clear();
    }
    
    private static void OnUpdate()
    {
        if (!StaticUnityMethods.IsPlaying)
            return;
        
        AllGrenades.ForEach(nade =>
        {
            if (!nade.IsSpawned || nade.IsDetonated || nade.Pickup is null)
                return;

            if (nade.RemainingTime == -1f)
                return;

            if (nade.RemainingTime != 0f)
            {
                nade.RemainingTime -= Time.deltaTime;

                if (nade.RemainingTime <= 0f)
                    nade.RemainingTime = 0f;
                else
                    return;
            }

            if (!nade.OnDetonating())
                return;
            
            nade.IsDetonated = true;
            
            CustomItemManager.PickupItems.Remove(nade.Pickup);
            
            nade.Pickup.DestroySelf();
            nade.Pickup = null;
            
            nade.OnDetonated();
            nade.OnDisabled();
            
            toRemove.Add(nade);
        });
        
        toRemove.ForEach(nade => AllGrenades.Remove(nade));
        toRemove.Clear();
    }

    [LoaderInitialize(2)]
    private static void OnInit()
    {
        CustomItemManager.OnItemCreated += OnItemCreated;
        CustomItemManager.OnItemDestroyed += OnItemDestroyed;
        
        StaticUnityMethods.OnFixedUpdate += OnUpdate;
        
        InternalEvents.OnRoundWaiting += OnWaiting;
    }
}