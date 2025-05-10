using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Attributes;

using LabExtended.Core.Networking;

using LabExtended.Events;
using LabExtended.Extensions;
using LabExtended.Patches.Functions.Items;

using Mirror;

namespace LabExtended.Utilities;

/// <summary>
/// Used to track item serials.
/// </summary>
public class ItemTracker : IDisposable
{
    private static readonly List<ItemTracker> trackerBuffer = new();
    
    /// <summary>
    /// Gets a list of item trackers.
    /// </summary>
    public static Dictionary<ushort, ItemTracker> Trackers { get; } = new();

    /// <summary>
    /// Gets a list of trackers for items with no serial number.
    /// </summary>
    public static Dictionary<object, ItemTracker> UnassignedTrackers { get; } = new();

    /// <summary>
    /// Gets called when a tracker is created.
    /// </summary>
    public static event Action<ItemTracker>? Created;
    
    /// <summary>
    /// Gets called when a tracker is destroyed.
    /// </summary>
    public static event Action<ItemTracker>? Destroyed;

    /// <summary>
    /// Gets called when a tracked item is selected.
    /// </summary>
    public static event Action<ItemTracker>? Selected;

    /// <summary>
    /// Gets called when a tracked item is de-selected.
    /// </summary>
    public static event Action<ItemTracker>? Deselected;

    /// <summary>
    /// Gets called when a tracked item is dropped.
    /// </summary>
    public static event Action<ItemTracker>? Dropped;

    /// <summary>
    /// Gets called when a tracked item is picked up.
    /// </summary>
    public static event Action<ItemTracker>? PickedUp;

    /// <summary>
    /// Gets called when a tracked item's owner is changed.
    /// </summary>
    public static event Action<ItemTracker>? OwnerChanged;

    /// <summary>
    /// Gets called when a serial number of a tracker is changed.
    /// </summary>
    public static event Action<ItemTracker>? SerialChanged; 
    
    /// <summary>
    /// Gets the tracked item serial.
    /// </summary>
    public ushort ItemSerial { get; private set; }
    
    /// <summary>
    /// Whether or not the item is selected.
    /// </summary>
    public bool IsSelected { get; internal set; }
    
    /// <summary>
    /// Whether or not the tracker has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    
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
    /// Gets the custom item instance associated with this item.
    /// </summary>
    public CustomItemInstance? CustomItem { get; internal set; }
    
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
        
        if (ItemSerial != 0)
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

        Pickup = item;
        ItemSerial = item.Info.Serial;
        
        if (item.PreviousOwner.Hub != null)
            Owner = ExPlayer.Get(item.PreviousOwner.Hub);
        
        Storage = new(false);
        
        if (ItemSerial != 0)
            Trackers.Add(ItemSerial, this);
        
        Created?.InvokeSafe(this);
    }

    /// <summary>
    /// Destroys this tracker and all associated items.
    /// </summary>
    public void Destroy()
    {
        if (Item != null)
            Item.DestroyItem();
        
        if (Pickup != null)
            Pickup.DestroySelf();
    }

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (!IsDisposed)
        {
            Trackers.Remove(ItemSerial);

            if (Item != null)
                UnassignedTrackers.Remove(Item);
            
            if (Pickup != null)
                UnassignedTrackers.Remove(Pickup);
            
            Destroyed?.InvokeSafe(this);

            Storage?.Dispose();
            Storage = null;

            Data = null;
            Item = null;
            Owner = null;
            Pickup = null;
            CustomItem = null;

            ItemSerial = 0;

            IsDisposed = true;
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
        
        PickedUp?.InvokeSafe(this);

        IsSelected = false;
        
        if (ItemSerial == 0 && item.ItemSerial != 0)
            SetSerial(item.ItemSerial);
    }

    internal void SetPickup(ItemPickupBase pickup, ExPlayer? owner = null)
    {
        if (Item != null && CustomItem != null && Owner != null)
        {
            if (Owner.Inventory.heldCustomItem != null && Owner.Inventory.heldCustomItem == CustomItem)
                Owner.Inventory.heldCustomItem = null;

            Owner.customItems.Remove(Item);
        }
        
        Item = null;

        Pickup = pickup;
        Owner = owner;
        
        Dropped?.InvokeSafe(this);

        IsSelected = false;
        
        if (ItemSerial == 0 && pickup.Info.Serial != 0)
            SetSerial(pickup.Info.Serial);
    }

    internal void SetOwner(ExPlayer owner)
    {
        var curOwner = Owner;
        
        Owner = owner;
        OwnerChanged?.InvokeSafe(this);

        if (CustomItem is null)
            return;
        
        if (curOwner != null)
        {
            if (curOwner.Inventory.heldCustomItem != null && curOwner.Inventory.heldCustomItem == CustomItem)
                curOwner.Inventory.heldCustomItem = null;

            if (Item != null)
                curOwner.customItems.Remove(Item);
        }

        if (Owner != null)
        {
            if (IsSelected)
                Owner.Inventory.heldCustomItem = CustomItem;
            
            if (Item != null)
                Owner.customItems[Item] = CustomItem;
        }
    }

    internal void SetSerial(ushort serial)
    {
        if (serial == ItemSerial)
            return;
        
        var curSerial = ItemSerial;

        if (curSerial != 0)
        {
            Trackers.Remove(curSerial);
        }
        else if (serial != 0)
        {
            if (Item != null)
                UnassignedTrackers.Remove(Item);

            if (Pickup != null)
                UnassignedTrackers.Remove(Pickup);
        }
        else if (serial == 0)
        {
            if (Item != null)
                UnassignedTrackers[Item] = this;
            
            if (Pickup != null)
                UnassignedTrackers[Pickup] = this;
        }

        ItemSerial = serial;
        
        if (serial != 0)
            Trackers[ItemSerial] = this;
        
        SerialChanged?.InvokeSafe(this);
    }

    private static void OnSpawned(NetworkIdentity identity)
    {
        if (identity.gameObject.TryFindComponent<ItemPickupBase>(out var pickup) 
            && !pickup.NetworkInfo.ItemId.IsAmmo())
        {
            if (pickup.Info.Serial != 0)
            {
                if (Trackers.ContainsKey(pickup.Info.Serial))
                    return;

                _ = new ItemTracker(pickup);
            }
            else
            {
                UnassignedTrackers[pickup] = new ItemTracker(pickup);
            }
        }
    }
    
    private static void OnCreated(ItemBase item)
    {
        if (item.ItemSerial != 0)
        {
            if (Trackers.ContainsKey(item.ItemSerial))
                return;
            
            if (UnassignedTrackers.TryGetValue(item, out var tracker))
                tracker.SetSerial(item.ItemSerial);
            else
                _ = new ItemTracker(item);
        }
        else
        {
            if (item.ItemSerial != 0 && UnassignedTrackers.TryGetValue(item, out var tracker))
                tracker.SetSerial(item.ItemSerial);
            else
                UnassignedTrackers[item] = new ItemTracker(item);
        }
    }

    private static void OnRestart()
    {
        trackerBuffer.Clear();
        
        foreach (var tracker in Trackers)
            trackerBuffer.Add(tracker.Value);
        
        foreach (var tracker in UnassignedTrackers)
            trackerBuffer.Add(tracker.Value);
        
        foreach (var tracker in trackerBuffer)
            tracker.Dispose();
        
        trackerBuffer.Clear();
        
        Trackers.Clear();
        UnassignedTrackers.Clear();
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        MirrorEvents.Spawning += OnSpawned;
        CreateItemPatch.Created += OnCreated;
        InternalEvents.OnRoundRestart += OnRestart;
    }
}