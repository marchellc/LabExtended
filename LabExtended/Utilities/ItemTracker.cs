using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Patches.Functions.Items;
using LabExtended.Utilities.Firearms;

using Mirror;

namespace LabExtended.Utilities;

/// <summary>
/// Used to track item serials.
/// </summary>
public class ItemTracker : IDisposable
{
    private static readonly Dictionary<ushort, ItemTracker> trackerBuffer = new();
    
    /// <summary>
    /// Gets a list of item trackers.
    /// </summary>
    public static Dictionary<ushort, ItemTracker> Trackers { get; } = new();

    /// <summary>
    /// Gets called when a tracker is created.
    /// </summary>
    public static event Action<ItemTracker>? Created;
    
    /// <summary>
    /// Gets called when a tracker is destroyed.
    /// </summary>
    public static event Action<ItemTracker>? Destroyed;
    
    /// <summary>
    /// Gets called once the item's owner selects it in their inventory.
    /// </summary>
    public static event Action<ItemTracker>? Selected;

    /// <summary>
    /// Gets called once the item's owner selects a different item.
    /// </summary>
    public static event Action<ItemTracker>? Deselected;

    /// <summary>
    /// Gets called once the item is picked up.
    /// </summary>
    public static event Action<ItemTracker>? PickedUp;

    /// <summary>
    /// Gets called once the item is dropped.
    /// </summary>
    public static event Action<ItemTracker>? Dropped;
    
    /// <summary>
    /// Gets the tracked item serial.
    /// </summary>
    public ushort ItemSerial { get; private set; }
    
    /// <summary>
    /// Whether or not the item is selected.
    /// </summary>
    public bool IsSelected { get; internal set; }
    
    /// <summary>
    /// Gets the player that currently owns the item.
    /// </summary>
    public ExPlayer? Owner { get; internal set; }
    
    /// <summary>
    /// Gets the inventory item instance.
    /// </summary>
    public ItemBase? Item { get; internal set; }
    
    /// <summary>
    /// Gets the dropped item instance.
    /// </summary>
    public ItemPickupBase? Pickup { get; internal set; }
    
    /// <summary>
    /// Gets the item's data storage.
    /// </summary>
    public PlayerStorage? Storage { get; internal set; }
    
    /// <summary>
    /// Gets the firearm's module cache. Will be null if the item is not a firearm.
    /// </summary>
    public FirearmModuleCache? FirearmModules { get; internal set; }
    
    /// <summary>
    /// Gets or sets the item tracker's custom data.
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// Creates a new <see cref="ItemTracker"/> instance.
    /// </summary>
    /// <param name="item">The target inventory item.</param>
    public ItemTracker(ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (item.ItemSerial == 0)
            throw new Exception($"The item's serial has not been set.");

        Item = item;
        ItemSerial = item.ItemSerial;
        
        if (item.Owner != null)
            Owner = ExPlayer.Get(item.Owner);

        Storage = new(false);
        
        if (Item is Firearm firearm)
            FirearmModules = firearm.GetModules();
        
        Trackers.Add(ItemSerial, this);
        
        Created?.InvokeSafe(this);
    }
    
    /// <summary>
    /// Creates a new <see cref="ItemTracker"/> instance.
    /// </summary>
    /// <param name="item">The target dropped item.</param>
    public ItemTracker(ItemPickupBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        
        if (item.NetworkInfo.Serial == 0)
            throw new Exception($"The pickup's serial has not been set.");

        Pickup = item;
        ItemSerial = item.Info.Serial;
        
        if (item.PreviousOwner.Hub != null)
            Owner = ExPlayer.Get(item.PreviousOwner.Hub);
        
        Storage = new(false);
        
        Trackers.Add(ItemSerial, this);
        
        Created?.InvokeSafe(this);
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (ItemSerial != 0)
        {
            Trackers.Remove(ItemSerial);
            
            Destroyed?.InvokeSafe(this);

            Storage?.Dispose();
            Storage = null;

            Data = null;
            Item = null;
            Owner = null;
            Pickup = null;

            FirearmModules = null;

            ItemSerial = 0;
        }
    }

    internal void SetSelected(bool selected)
    {
        if (IsSelected == selected)
            return;
        
        IsSelected = selected;
        
        if (IsSelected)
            Selected?.InvokeSafe(this);
        else
            Deselected?.InvokeSafe(this);
    }

    internal void SetItem(ItemBase item, ExPlayer? owner = null)
    {
        Pickup = null;
        
        Item = item;
        Owner = owner;
        
        if (item is Firearm firearm)
            FirearmModules = firearm.GetModules();
        
        PickedUp?.InvokeSafe(this);

        IsSelected = false;
    }

    internal void SetPickup(ItemPickupBase pickup, ExPlayer? owner = null)
    {
        Item = null;

        Pickup = pickup;
        Owner = owner;
        
        Dropped?.InvokeSafe(this);

        FirearmModules = null;

        IsSelected = false;
    }

    private static void OnSpawned(NetworkIdentity identity)
    {
        if (identity.gameObject.TryFindComponent<ItemPickupBase>(out var pickup) 
            && pickup.NetworkInfo.Serial != 0
            && !pickup.NetworkInfo.ItemId.IsAmmo()
            && !Trackers.ContainsKey(pickup.NetworkInfo.Serial))
        {
            _ = new ItemTracker(pickup);
        }
    }
    
    private static void OnCreated(ItemBase item)
    {
        if (item.ItemSerial != 0 && !Trackers.ContainsKey(item.ItemSerial))
        {
            _ = new ItemTracker(item);
        }
    }

    private static void OnRestart()
    {
        trackerBuffer.Clear();
        
        foreach (var tracker in Trackers)
            trackerBuffer.Add(tracker.Key, tracker.Value);
        
        foreach (var tracker in trackerBuffer)
            tracker.Value.Dispose();
        
        trackerBuffer.Clear();
        Trackers.Clear();
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        MirrorEvents.Spawning += OnSpawned;
        CreateItemPatch.Created += OnCreated;
        InternalEvents.OnRoundRestart += OnRestart;
    }
}