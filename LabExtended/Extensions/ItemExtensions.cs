using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.DebugTools;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp1344;

using LabExtended.API;
using LabExtended.Utilities;

using Mirror;

using UnityEngine;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.Extensions;

/// <summary>
/// A class that holds extensions for the <see cref="ItemBase"/> and <see cref="Inventory"/> class.
/// </summary>
public static class ItemExtensions
{
    /// <summary>
    /// A dictionary that contains all item prefabs.
    /// </summary>
    public static Dictionary<ItemType, ItemBase> Prefabs => InventoryItemLoader._loadedItems;

    /// <summary>
    /// Whether or not all items prefabs have been loaded.
    /// </summary>
    public static bool PrefabsLoaded => Prefabs is { Count: > 0 };

    /// <summary>
    /// Reloads item prefabs.
    /// </summary>
    public static void ReloadPrefabs()
        => InventoryItemLoader.ForceReload();

    /// <summary>
    /// Gets the current inventory slot of an item.
    /// </summary>
    /// <param name="item">The target item.</param>
    /// <returns>The item slot number (1 - 8 or 0 if the item does not have an owner)</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static byte GetInventorySlot(this ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (item.Owner == null)
            return 0;

        return (byte)(item.OwnerInventory.UserInventory.Items.FindKeyIndex(item.ItemSerial) + 1);
    }

    /// <summary>
    /// Tries to get a prefab of an item.
    /// </summary>
    /// <param name="type">The type of item.</param>
    /// <param name="prefab">The found prefab instance.</param>
    /// <typeparam name="T">The type to cast the prefab to.</typeparam>
    /// <returns>true if the prefab was retrieved and cast</returns>
    public static bool TryGetItemPrefab<T>(this ItemType type, out T? prefab) where T : ItemBase
    {
        if (!PrefabsLoaded)
            ReloadPrefabs();

        if (!Prefabs.TryGetValue(type, out var item))
        {
            prefab = null;
            return false;
        }

        if (item is not T castPrefab)
        {
            prefab = null;
            return false;
        }

        prefab = castPrefab;
        return true;
    }

    /// <summary>
    /// Attempts to get the prefab of an item type.
    /// </summary>
    /// <param name="type">The type to get.</param>
    /// <param name="prefab">The found prefab instance.</param>
    /// <returns>true if the prefab was found</returns>
    public static bool TryGetItemPrefab(this ItemType type, out ItemBase prefab)
    {
        if (!PrefabsLoaded)
            ReloadPrefabs();

        return Prefabs.TryGetValue(type, out prefab);
    }

    /// <summary>
    /// Gets an item prefab.
    /// </summary>
    /// <typeparam name="T">The type of the item to get.</typeparam>
    /// <param name="itemType">The type of the item to get.</param>
    /// <returns>The <see cref="ItemBase"/> prefab instance if found, otherwise <see langword="null"/>.</returns>
    public static T? GetItemPrefab<T>(this ItemType itemType) where T : ItemBase
        => TryGetItemPrefab<T>(itemType, out var prefab) ? prefab : null;

    /// <summary>
    /// Gets an instance of an item.
    /// </summary>
    /// <typeparam name="T">The type of the item to get.</typeparam>
    /// <param name="itemType">The type of the item to get.</param>
    /// <returns>The item's instance, if succesfull. Otherwise <see langword="null"/>.</returns>
    public static T? GetItemInstance<T>(this ItemType itemType, ushort? serial = null) where T : ItemBase
    {
        if (!TryGetItemPrefab<T>(itemType, out var result))
            return null;

        var item = UnityEngine.Object.Instantiate(result);

        if (serial.HasValue)
            item.ItemSerial = serial.Value;
        else
            item.ItemSerial = ItemSerialGenerator.GenerateNext();

        return item;
    }

    /// <summary>
    /// Gets a pickup instance of an item.
    /// </summary>
    /// <typeparam name="T">The type of the pickup instance to get.</typeparam>
    /// <param name="itemType">The type of the pickup instance to get.</param>
    /// <param name="position">The position to spawn the pickup at.</param>
    /// <param name="scale">The scale of the pickup item.</param>
    /// <param name="rotation">The rotation of the pickup item.</param>
    /// <param name="serial">The item's serial. If <see langword="null"/> a new one will be generated.</param>
    /// <param name="spawnPickup">Whether or not to spawn the pickup for players.</param>
    /// <returns>The pickup instance, if found. Otherwise <see langword="null"/>.</returns>
    public static T? GetPickupInstance<T>(this ItemType itemType, Vector3? position = null, Vector3? scale = null,
        Quaternion? rotation = null, ushort? serial = null, bool spawnPickup = false) where T : ItemPickupBase
    {
        if (!TryGetItemPrefab(itemType, out var itemBase))
            return null;

        if (itemBase.PickupDropModel is null)
            return null;

        var pickup = UnityEngine.Object.Instantiate(itemBase.PickupDropModel,
            position ?? Vector3.zero,
            rotation ?? Quaternion.identity);

        pickup.NetworkInfo =
            new PickupSyncInfo(itemType, itemBase.Weight, serial ?? ItemSerialGenerator.GenerateNext());

        if (position.HasValue)
            pickup.Position = position.Value;

        if (rotation.HasValue)
            pickup.Rotation = rotation.Value;

        if (scale.HasValue)
            pickup.transform.localScale = scale.Value;

        if (spawnPickup)
            NetworkServer.Spawn(pickup.gameObject);

        return (T)pickup;
    }

    /// <summary>
    /// Attempts to retrieve a pickup's rigidbody component.
    /// </summary>
    /// <param name="pickup">The target pickup.</param>
    /// <param name="rigidbody">The retrieved rigidbody component.</param>
    /// <returns>true if the component was retrieved</returns>
    public static bool TryGetRigidbody(this ItemPickupBase pickup, out Rigidbody? rigidbody)
        => (rigidbody = GetRigidbody(pickup)) != null;

    /// <summary>
    /// Gets the pickup's <see cref="Rigidbody"/> component.
    /// </summary>
    /// <param name="itemPickupBase">The pickup to get a <see cref="Rigidbody"/> from.</param>
    /// <returns>The <see cref="Rigidbody"/> component instance if found, otherwise <see langword="null"/>.</returns>
    public static Rigidbody? GetRigidbody(this ItemPickupBase itemPickupBase)
    {
        if (itemPickupBase is null)
            return null;

        if (itemPickupBase.PhysicsModule is PickupStandardPhysics standardPhysics)
            return standardPhysics.Rb;

        return itemPickupBase.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Applies effects granted to an item's owner once the item is dropped.
    /// </summary>
    /// <param name="item">The item to simulate drop of.</param>
    /// <param name="keepItem">Whether or not the item should be kept in the player's inventory.</param>
    /// <returns>true if a pickup of the item should be dropped</returns>
    public static bool SimulateDrop(this ItemBase item, out bool keepItem)
    {
        keepItem = false;

        if (item != null && item.Owner != null)
        {
            if (item is Scp1344Item scp1344 && scp1344 != null)
            {
                if (scp1344.Status is Scp1344Status.Deactivating)
                {
                    keepItem = true;
                    return false;
                }

                if (scp1344.Status is Scp1344Status.Active)
                {
                    scp1344.ServerSetStatus(Scp1344Status.Dropping);

                    keepItem = true;
                    return false;
                }
            }

            if (item is ParticleDisruptor particleDisruptor && particleDisruptor.DeleteOnDrop)
                return false;

            if (item is SingleUseKeycardItem singleUseKeycard && singleUseKeycard._destroyed)
                return false;

            if (item is RagdollMover)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Freezes the specified pickup.
    /// </summary>
    /// <param name="itemPickupBase">The pickup to freeze.</param>
    /// <returns><see langword="true"/> if the pickup was successfully frozen, otherwise <see langword="false"/>.</returns>
    public static bool FreezePickup(this ItemPickupBase itemPickupBase)
    {
        if (itemPickupBase.PhysicsModule is not PickupStandardPhysics pickupStandardPhysics)
            return false;

        if (pickupStandardPhysics.Rb is null)
            return false;

        pickupStandardPhysics.Rb.isKinematic = true;
        pickupStandardPhysics.Rb.constraints = RigidbodyConstraints.FreezeAll;

        pickupStandardPhysics.ClientFrozen = true;
        return true;
    }

    /// <summary>
    /// Unfreezes the specified pickup.
    /// </summary>
    /// <param name="itemPickupBase">The pickup to unfreeze.</param>
    /// <returns><see langword="true"/> if the pickup was successfully unfrozen, otherwise <see langword="false"/>.</returns>
    public static bool UnfreezePickup(this ItemPickupBase itemPickupBase)
    {
        if (itemPickupBase.PhysicsModule is not PickupStandardPhysics pickupStandardPhysics)
            return false;

        if (pickupStandardPhysics?.Rb == null)
            return false;

        pickupStandardPhysics.Rb.isKinematic = false;
        pickupStandardPhysics.Rb.constraints = RigidbodyConstraints.None;

        pickupStandardPhysics.ClientFrozen = false;
        return true;
    }

    /// <summary>
    /// Unlocks the specified pickup.
    /// </summary>
    /// <param name="itemPickupBase">The pickup to unlock.</param>
    public static void UnlockPickup(this ItemPickupBase itemPickupBase)
    {
        if (itemPickupBase is null)
            return;

        var info = itemPickupBase.Info;

        info.Locked = false;
        info.InUse = false;

        itemPickupBase.NetworkInfo = info;
    }

    /// <summary>
    /// Locks the specified pickup.
    /// </summary>
    /// <param name="itemPickupBase">The pickup to lock.</param>
    public static void LockPickup(this ItemPickupBase itemPickupBase)
    {
        if (itemPickupBase is null)
            return;

        var info = itemPickupBase.Info;

        info.Locked = true;
        info.InUse = true;

        itemPickupBase.NetworkInfo = info;
    }

    /// <summary>
    /// Transfers an item from one player's inventory to another.
    /// </summary>
    /// <param name="item">The item to transfer.</param>
    /// <param name="newOwner">The player to transfer the item to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void TransferItem(this ItemBase item, ReferenceHub newOwner)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        
        if (newOwner == null)
            throw new ArgumentNullException(nameof(newOwner));

        if (item.Owner != null)
        {
            if (item.OwnerInventory.CurInstance == item)
            {
                item.OwnerInventory.CurInstance = null;
                item.OwnerInventory.NetworkCurItem = ItemIdentifier.None;
            }

            item.OwnerInventory.UserInventory.Items.Remove(item.ItemSerial);
            item.OwnerInventory.SendItemsNextFrame = true;
            
            item.OnRemoved(null);
        }

        item.Owner = newOwner;
        
        if (item.Owner != null)
            item.Owner.inventory.UserInventory.Items[item.ItemSerial] = item;
        
        item.OnAdded(null);

        if (item.Owner != null)
        {
            if (newOwner.isLocalPlayer && item is IAcquisitionConfirmationTrigger
                {
                    AcquisitionAlreadyReceived: false
                } acquisitionConfirmationTrigger)
            {
                acquisitionConfirmationTrigger.ServerConfirmAcqusition();
                acquisitionConfirmationTrigger.AcquisitionAlreadyReceived = true;
            }

            item.OwnerInventory.SendItemsNextFrame = true;
        }
    }
    
    /// <summary>
    /// Destroys an item instance.
    /// </summary>
    /// <param name="item">The item instance to destroy.</param>
    public static void DestroyItem(this ItemBase item)
    {
        if (item.Owner != null)
        {
            if (item.OwnerInventory.CurItem.SerialNumber == item.ItemSerial)
            {
                item.OwnerInventory.CurInstance = null;
                item.OwnerInventory.NetworkCurItem = ItemIdentifier.None;
            }

            item.OwnerInventory.UserInventory.Items.Remove(item.ItemSerial);
            item.OwnerInventory.SendItemsNextFrame = true;
            
            item.OnRemoved(null);
        }

        UnityEngine.Object.Destroy(item.gameObject);
    }

    /// <summary>
    /// Gets a list of players that have a specific item type.
    /// </summary>
    /// <param name="item">The item type.</param>
    /// <returns>A list of players.</returns>
    public static IEnumerable<ExPlayer> GetPlayers(this ItemType item)
        => ExPlayer.Players.Where(p => p?.Inventory != null && p.Inventory.HasItem(item));

    /// <summary>
    /// Enumerates a list of players that have a specific item type.
    /// </summary>
    /// <param name="item">The item type.</param>
    /// <param name="action">The delegate to invoke.</param>
    public static void ForEach(this ItemType item, Action<ExPlayer> action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var player in ExPlayer.Players)
        {
            if (player?.Inventory == null)
                continue;
            
            if (!player.Inventory.HasItem(item))
                continue;

            action(player);
        }
    }

    /// <summary>
    /// Enumerates a list of players that have a specific item type.
    /// </summary>
    /// <param name="type">The item type.</param>
    /// <param name="action">The delegate to invoke.</param>
    /// <typeparam name="T">The type to cast the item to.</typeparam>
    public static void ForEach<T>(this ItemType type, Action<ExPlayer, T> action) where T : ItemBase
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));
        
        foreach (var player in ExPlayer.Players)
        {
            if (player?.Inventory == null)
                continue;

            foreach (var item in player.Inventory.Items)
            {
                if (item.ItemTypeId != type || item is not T value)
                    continue;
                
                action(player, value);
            }
        }
    }

    /// <summary>
    /// Gets or creates a new tracker for an item.
    /// </summary>
    /// <param name="item">The target item.</param>
    /// <returns>The found or created tracker.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static ItemTracker GetTracker(this ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (item.ItemSerial == 0)
            throw new Exception("Item's serial has not been set");

        if (item.TryGetTracker(out var tracker))
            return tracker;

        return new(item);
    }
    
    /// <summary>
    /// Gets or creates a new tracker for a pickup.
    /// </summary>
    /// <param name="item">The target pickup.</param>
    /// <returns>The found or created tracker.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static ItemTracker GetTracker(this ItemPickupBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (item.Info.Serial == 0)
            throw new Exception("Pickup's serial has not been set");

        if (item.TryGetTracker(out var tracker))
            return tracker;

        return new(item);
    }

    /// <summary>
    /// Attempts to get the tracker of an item.
    /// </summary>
    /// <param name="item">The target item.</param>
    /// <param name="tracker">The found tracker.</param>
    /// <returns>true if the tracker was found or created</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetTracker(this ItemBase item, out ItemTracker tracker)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));
        
        if (ItemTracker.Trackers.TryGetValue(item.ItemSerial, out tracker))
            return true;

        if (item.ItemSerial != 0)
            tracker = new(item);

        return tracker != null;
    }
    
    /// <summary>
    /// Attempts to get the tracker of a pickup.
    /// </summary>
    /// <param name="pickup">The target pickup.</param>
    /// <param name="tracker">The found tracker.</param>
    /// <returns>true if the tracker was found or created</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetTracker(this ItemPickupBase pickup, out ItemTracker tracker)
    {
        if (pickup == null)
            throw new ArgumentNullException(nameof(pickup));

        if (ItemTracker.Trackers.TryGetValue(pickup.NetworkInfo.Serial, out tracker))
            return true;

        if (pickup.NetworkInfo.Serial != 0)
            tracker = new(pickup);

        return tracker != null;
    }
    
    /// <summary>
    /// Whether or not a given item type is an ammo.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>true if the item type is ammo</returns>
    public static bool IsAmmo(this ItemType type)
        => type.TryGetItemPrefab(out var prefab) && prefab.Category is ItemCategory.Ammo;
}