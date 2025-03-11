using InventorySystem.Items.Usables;

using LabExtended.API.CustomItems;
using LabExtended.Attributes;
using LabExtended.Events;

using UnityEngine;
using Utils.Networking;

namespace LabExtended.API.CustomUsables;

/// <summary>
/// Manages Custom Usable items.
/// </summary>
public static class CustomUsableManager
{
    /// <summary>
    /// Gets all usable items.
    /// </summary>
    public static List<CustomUsableInstance> AllUsables { get; } = new();

    /// <summary>
    /// Starts using a specific Custom Usable item.
    /// </summary>
    /// <param name="customUsableInstance">The item to use.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void UseItem(CustomUsableInstance customUsableInstance)
    {
        if (customUsableInstance is null)
            throw new ArgumentNullException(nameof(customUsableInstance));

        if (customUsableInstance.IsUsing)
            throw new Exception("This Custom Usable is already being used.");

        if (customUsableInstance.Item is null)
            throw new Exception("This Custom Usable is not in inventory.");

        customUsableInstance.OnStartUsing();
        
        customUsableInstance.IsUsing = true;

        customUsableInstance.RemainingCooldown = 0f;
        customUsableInstance.RemainingTime = customUsableInstance.CustomData.UseTime;

        customUsableInstance.OnStartedUsing();
        
        new StatusMessage(StatusMessage.StatusType.Start, customUsableInstance.ItemSerial).SendToAuthenticated();
    }

    /// <summary>
    /// Stops using a specific Custom Usable item.
    /// </summary>
    /// <param name="customUsableInstance">The item to stop using.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static void StopItem(CustomUsableInstance customUsableInstance)
    {
        if (customUsableInstance is null)
            throw new ArgumentNullException(nameof(customUsableInstance));
        
        if (!customUsableInstance.IsUsing)
            throw new Exception("This Custom Usable is not being used.");
        
        if (customUsableInstance.Item is null)
            throw new Exception("This Custom Usable is not in inventory.");

        customUsableInstance.OnCancelling();
        
        customUsableInstance.IsUsing = false;

        customUsableInstance.RemainingTime = 0f;
        customUsableInstance.RemainingCooldown = customUsableInstance.CustomData.Cooldown;
        
        customUsableInstance.OnCancelled();
        
        new StatusMessage(StatusMessage.StatusType.Cancel, customUsableInstance.ItemSerial).SendToAuthenticated();
    }

    private static void OnItemCreated(CustomItemInstance customItemInstance)
    {
        if (customItemInstance is CustomUsableInstance customUsableInstance)
            AllUsables.Add(customUsableInstance);
    }

    private static void OnItemRemoved(CustomItemInstance customItemInstance)
    {
        if (customItemInstance is CustomUsableInstance customUsableInstance)
            AllUsables.Remove(customUsableInstance);
    }

    private static void OnWaiting()
    {
        AllUsables.Clear();
    }

    private static void OnUpdate()
    {
        if (!StaticUnityMethods.IsPlaying)
            return;
        
        AllUsables.ForEach(item =>
        {
            if (item.Item is null)
                return;

            if (item.RemainingCooldown != 0f)
            {
                item.RemainingCooldown -= Time.deltaTime;

                if (item.RemainingCooldown <= 0f)
                    item.RemainingCooldown = 0f;
            }
            
            if (!item.IsUsing)
                return;

            if (!item.IsHeld)
            {
                item.OnCancelling();
                
                item.IsUsing = false;

                item.RemainingTime = 0f;
                item.RemainingCooldown = item.CustomData.Cooldown;
                
                item.OnCancelled();
                
                new StatusMessage(StatusMessage.StatusType.Cancel, item.ItemSerial).SendToAuthenticated();
                return;
            }

            item.RemainingTime -= Time.deltaTime;

            if (item.RemainingTime <= 0f)
            {
                item.IsUsing = false;
                
                item.RemainingTime = 0f;
                item.RemainingCooldown = item.CustomData.Cooldown;
                
                item.OnCompleted();
            }
        });
    }
    
    [LoaderInitialize(2)]
    private static void OnInit()
    {
        CustomItemManager.OnItemCreated += OnItemCreated;
        CustomItemManager.OnItemDestroyed += OnItemRemoved;
        
        StaticUnityMethods.OnFixedUpdate += OnUpdate;
        
        InternalEvents.OnRoundWaiting += OnWaiting;
    }
}