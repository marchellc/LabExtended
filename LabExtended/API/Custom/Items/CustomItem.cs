using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Arguments.Scp914Events;

using LabApi.Events.Handlers;
using LabApi.Features.Wrappers;

using LabExtended.API.Custom.Items.Enums;
using LabExtended.API.Custom.Items.Events;

using LabExtended.Core;
using LabExtended.Core.Configs.Objects;

using LabExtended.Events;
using LabExtended.Events.Map;
using LabExtended.Events.Player;

using LabExtended.Extensions;

using System.ComponentModel;

using UnityEngine;

using YamlDotNet.Serialization;

namespace LabExtended.API.Custom.Items
{
    /// <summary>
    /// Base class for custom item implementations.
    /// </summary>
    public abstract class CustomItem
    {
        #region Delegates
        /// <summary>
        /// Represents a method that is called for each item held by a player, allowing inspection or modification of
        /// item-specific data.
        /// </summary>
        /// <param name="item">The item being processed. Represents the current item held by the player.</param>
        /// <param name="owner">The player who owns the item.</param>
        /// <param name="itemData">A reference to an object containing additional data associated with the item. Can be modified within the
        /// delegate to update item-specific information.</param>
        public delegate void ForEachItemDelegate(ItemBase item, ExPlayer owner, ref object? itemData);

        /// <summary>
        /// Represents a method that is called for each item held by a player, allowing inspection or modification of
        /// associated item data.
        /// </summary>
        /// <remarks>Use this delegate to perform actions or update item data for each item held by a
        /// player, typically in collection iteration scenarios.</remarks>
        /// <typeparam name="T">The type of the item data associated with each held item. Can be a reference or value type.</typeparam>
        /// <param name="item">The item being processed during the iteration.</param>
        /// <param name="owner">The player who is owns the item.</param>
        /// <param name="itemData">A reference to the item data associated with the held item. The value can be read or modified within the
        /// delegate.</param>
        public delegate void ForEachItemDelegate<T>(ItemBase item, ExPlayer owner, ref T? itemData);

        /// <summary>
        /// Represents a method that is called for each item held by a player, allowing inspection or modification of
        /// item-specific data.
        /// </summary>
        /// <param name="item">The item being processed. Represents the current item held by the player.</param>
        /// <param name="owner">The player who owns the item.</param>
        /// <param name="itemData">A reference to an object containing additional data associated with the item. Can be modified within the
        /// delegate to update item-specific information.</param>
        public delegate void ForEachPickupDelegate(ItemPickupBase item, ref object? itemData);

        /// <summary>
        /// Represents a method that is called for each item held by a player, allowing inspection or modification of
        /// associated item data.
        /// </summary>
        /// <remarks>Use this delegate to perform actions or update item data for each item held by a
        /// player, typically in collection iteration scenarios.</remarks>
        /// <typeparam name="T">The type of the item data associated with each held item. Can be a reference or value type.</typeparam>
        /// <param name="item">The item being processed during the iteration.</param>
        /// <param name="itemData">A reference to the item data associated with the held item. The value can be read or modified within the
        /// delegate.</param>
        public delegate void ForEachPickupDelegate<T>(ItemPickupBase item, ref T? itemData);
        #endregion

        private static Dictionary<string, CustomItem> itemsById = new();

        /// <summary>
        /// Gets all registered custom items.
        /// </summary>
        public static IReadOnlyDictionary<string, CustomItem> RegisteredItems => itemsById;

        #region Events
        /// <summary>
        /// Gets called when a new custom item is registered.
        /// </summary>
        public static event Action<CustomItem>? Registered;

        /// <summary>
        /// Gets called when a custom item is unregistered.
        /// </summary>
        public static event Action<CustomItem>? Unregistered;

        /// <summary>
        /// Gets called when a custom item is added to a player's inventory.
        /// </summary>
        public static event Action<CustomItemAddedEventArgs>? ItemAdded;

        /// <summary>
        /// Gets called when a new custom item pickup is spawned.
        /// </summary>
        public static event Action<CustomItemSpawnedEventArgs>? ItemSpawned;

        /// <summary>
        /// Gets called when a custom item is dropped.
        /// </summary>
        public static event Action<CustomItemDroppedEventArgs>? ItemDropped;

        /// <summary>
        /// Gets called when an active custom item instance is destroyed.
        /// </summary>
        public static event Action<CustomItemDestroyedEventArgs>? ItemDestroyed;
        #endregion

        /// <summary>
        /// Determines whether the specified item serial corresponds to a custom item and retrieves the associated
        /// custom item if found.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to check for a custom item association.</param>
        /// <param name="customItem">When this method returns, contains the <see cref="CustomItem"/> associated with the specified item serial if
        /// found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns>true if a custom item matching the specified serial is found; otherwise, false.</returns>
        public static bool IsCustomItem(ushort itemSerial, out CustomItem customItem)
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value.Internal_CheckItem(itemSerial, out _))
                {
                    customItem = pair.Value;
                    return true;
                }
            }

            customItem = null!;
            return false;
        }

        /// <summary>
        /// Determines whether the specified item serial corresponds to a custom item and retrieves its associated data
        /// if found.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to check for a custom item association.</param>
        /// <param name="customItem">When this method returns, contains the <see cref="CustomItem"/> instance associated with the specified item
        /// serial if found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <param name="customData">When this method returns, contains the custom data associated with the item if found; otherwise, <see
        /// langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns>true if the specified item serial is associated with a custom item; otherwise, false.</returns>
        public static bool IsCustomItem(ushort itemSerial, out CustomItem customItem, out object? customData)
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value.Internal_CheckItem(itemSerial, out var tracker))
                {
                    customItem = pair.Value;
                    customData = tracker.Data;

                    return true;
                }
            }

            customItem = null!;
            customData = null;

            return false;
        }

        /// <summary>
        /// Determines whether the specified item serial corresponds to a custom item of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of custom item to search for. Must derive from <see cref="CustomItem"/>.</typeparam>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <param name="customItem">When this method returns, contains the custom item of type <typeparamref name="T"/> associated with the
        /// specified serial number, if found; otherwise, <see langword="null"/>.</param>
        /// <returns>true if a custom item of type <typeparamref name="T"/> with the specified serial number is found; otherwise,
        /// false.</returns>
        public static bool IsCustomItem<T>(ushort itemSerial, out T customItem) where T : CustomItem
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value is T castItem && pair.Value.Internal_CheckItem(itemSerial, out _))
                {
                    customItem = castItem;
                    return true;
                }
            }

            customItem = null!;
            return false;
        }

        /// <summary>
        /// Determines whether a custom item of the specified type exists for the given item serial and retrieves the
        /// associated item and data if found.
        /// </summary>
        /// <remarks>If multiple items of type <typeparamref name="T"/> exist, only the first matching
        /// item is returned. This method does not modify the underlying collection.</remarks>
        /// <typeparam name="T">The type of custom item to search for. Must derive from <see cref="CustomItem"/>.</typeparam>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <param name="customItem">When this method returns, contains the custom item of type <typeparamref name="T"/> if found; otherwise,
        /// <see langword="null"/>.</param>
        /// <param name="customData">When this method returns, contains the associated custom data if the item is found; otherwise, <see
        /// langword="null"/>.</param>
        /// <returns>true if a custom item of type <typeparamref name="T"/> with the specified serial exists; otherwise, false.</returns>
        public static bool IsCustomItem<T>(ushort itemSerial, out T customItem, out object? customData) where T : CustomItem
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value is T castItem && pair.Value.Internal_CheckItem(itemSerial, out var tracker))
                {
                    customItem = castItem;
                    customData = tracker.Data;

                    return true;
                }
            }

            customItem = null!;
            customData = null;

            return false;
        }

        /// <summary>
        /// Determines whether the specified item serial corresponds to a custom item of the given type and retrieves
        /// the associated item and data if found.
        /// </summary>
        /// <remarks>If multiple items match the specified type, only the first found is returned. This
        /// method does not throw exceptions if the item is not found; instead, the out parameters are set to their
        /// default values.</remarks>
        /// <typeparam name="TItem">The type of custom item to search for. Must inherit from CustomItem.</typeparam>
        /// <typeparam name="TData">The type of custom data to retrieve if the item is found.</typeparam>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <param name="customItem">When this method returns, contains the found custom item of type <typeparamref name="TItem"/> if the item
        /// exists; otherwise, <see langword="null"/>.</param>
        /// <param name="customData">When this method returns, contains the associated custom data of type <typeparamref name="TData"/> if
        /// available; otherwise, the default value for the type.</param>
        /// <returns>true if a custom item matching the specified serial and type is found; otherwise, false.</returns>
        public static bool IsCustomItem<TItem, TData>(ushort itemSerial, out TItem customItem, out TData? customData) where TItem : CustomItem
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value is TItem castItem 
                    && pair.Value.Internal_CheckItem(itemSerial, out var tracker))
                {
                    customItem = castItem;
                    customData = tracker.Data is TData castData
                        ? castData
                        : default;

                    return true;
                }
            }

            customItem = null!;
            customData = default;

            return false;
        }

        /// <summary>
        /// Determines whether the specified item is currently tracked and retrieves its associated tracking
        /// information.
        /// </summary>
        /// <remarks>If the method returns <see langword="true"/>, <paramref name="trackedItem"/> will
        /// contain the tracking data for the specified item. If the item is not tracked, <paramref name="trackedItem"/>
        /// will be null.</remarks>
        /// <param name="itemSerial">The unique serial number of the item to check for tracking.</param>
        /// <param name="trackedItem">When this method returns, contains the tracking information for the item if it is tracked; otherwise, null.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the item is tracked; otherwise, false.</returns>
        public static bool IsTrackedItem(ushort itemSerial, out TrackedCustomItem trackedItem)
        {
            foreach (var pair in itemsById)
            {
                if (pair.Value.Internal_CheckItem(itemSerial, out var tracker))
                {
                    trackedItem = tracker;
                    return true;
                }
            }

            trackedItem = null!;
            return false;
        }

        internal List<TrackedCustomItem> trackers = new();

        /// <summary>
        /// Gets the ID of the custom item.
        /// </summary>
        [YamlIgnore]
        public abstract string Id { get; }

        /// <summary>
        /// Gets the name of the custom item.
        /// </summary>
        [YamlIgnore]
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the weight of the item.
        /// </summary>
        /// <remarks>Values below zero use default values of the underlying item.</remarks>
        [Description("Sets the weight of the item (in kilograms), values below zero use default values.")]
        public virtual float Weight { get; set; } = -1f;

        /// <summary>
        /// Gets or sets the type of the item when dropped.
        /// </summary>
        [Description("Sets the type of the item when spawned or dropped (\"None\" prevents this item from spawning or being dropped).")]
        public abstract ItemType PickupType { get; set; }

        /// <summary>
        /// Gets or sets the type of the item when in inventory.
        /// </summary>
        [Description("Sets the type of the item when in inventory (\"None\" prevents this item from being in inventory).")]
        public abstract ItemType InventoryType { get; set; }

        /// <summary>
        /// Gets or sets the scale of the item.
        /// </summary>
        [Description("Sets the scale of the item when spawned or dropped.")]
        public virtual YamlVector3 Scale { get; set; } = new(Vector3.one);

        /// <summary>
        /// Gets or sets a value indicating whether or not the custom item should be dropped when a player leaves.
        /// </summary>
        [Description("Sets if custom items owned by a player should be dropped from the player's inventory when they leave.")]
        public virtual bool DropOnOwnerLeave { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether or not custom items dropped by a player be destroyed when the player who dropped them leaves.
        /// </summary>
        [Description("Sets if custom items dropped by a player should be destroyed when the player who dropped them leaves.")]
        public virtual bool DestroyOnOwnerLeave { get; set; } = false;

        /// <summary>
        /// Gets a list of all tracked custom item instances.
        /// </summary>
        [YamlIgnore]
        public IReadOnlyList<TrackedCustomItem> TrackedItems => trackers;

        /// <summary>
        /// Registers this custom item.
        /// </summary>
        /// <returns>true if the item was registered</returns>
        public bool Register()
        {
            if (itemsById.ContainsKey(Id))
                return false;

            itemsById.Add(Id, this);

            Registered?.Invoke(this);

            OnRegistered();

            ApiLog.Info("Custom Items", $"&2Registered&r custom item &3{Name}&r (&6{Id}&r)");
            return true;
        }

        /// <summary>
        /// Unregisters this custom item and removes all active instances.
        /// </summary>
        /// <returns></returns>
        public bool Unregister()
        {
            if (itemsById.Remove(Id))
            {
                DestroyInstances();

                Unregistered?.Invoke(this);

                OnUnregistered();

                ApiLog.Info("Custom Items", $"&1Unregistered&r custom item &3{Name}&r (&6{Id}&r)");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Invokes the specified delegate for each item currently held by a valid tracker and its associated owner.
        /// </summary>
        /// <remarks>Only items with valid trackers and owners are processed. The delegate may modify the
        /// item's associated data via the provided reference.</remarks>
        /// <param name="forEachItemDelegate">A delegate to execute for each held item. The delegate receives the item, its owner, and a reference to the
        /// item's associated data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachItemDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachHeldItem(ForEachItemDelegate forEachItemDelegate)
        {
            if (forEachItemDelegate is null)
                throw new ArgumentNullException(nameof(forEachItemDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Item == null
                    || tracker.Owner?.ReferenceHub == null
                    || tracker.Owner.Inventory.CurrentItemIdentifier.SerialNumber != tracker.TargetSerial)
                    continue;

                forEachItemDelegate(tracker.Item, tracker.Owner!, ref tracker.Data);
            }
        }

        /// <summary>
        /// Invokes the specified delegate for each item currently held by a valid tracker and its associated owner.
        /// </summary>
        /// <remarks>Only items with valid trackers and owners are processed. The delegate may modify the
        /// item's associated data via the provided reference.</remarks>
        /// <param name="forEachItemDelegate">A delegate to execute for each held item. The delegate receives the item, its owner, and a reference to the
        /// item's associated data.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachItemDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachHeldItem<T>(ForEachItemDelegate<T> forEachItemDelegate)
        {
            if (forEachItemDelegate is null)
                throw new ArgumentNullException(nameof(forEachItemDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Item == null
                    || tracker.Owner?.ReferenceHub == null
                    || tracker.Owner.Inventory.CurrentItemIdentifier.SerialNumber != tracker.TargetSerial)
                    continue;

                var dataTemp = tracker.Data is T castData
                    ? castData
                    : default;

                forEachItemDelegate(tracker.Item, tracker.Owner!, ref dataTemp);

                tracker.Data = dataTemp;
            }
        }

        /// <summary>
        /// Invokes the specified delegate for each active item in the collection, allowing callers to process or modify
        /// item data.
        /// </summary>
        /// <remarks>An item is considered active if its tracker passes validation and the item is not
        /// <see langword="null"/>. Modifications to the data parameter within the delegate will be persisted for each
        /// item.</remarks>
        /// <param name="forEachItemDelegate">A delegate to execute for each active item. The delegate receives the item, its owner, and a reference to
        /// the item's associated data, which can be modified.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachItemDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachActiveItem(ForEachItemDelegate forEachItemDelegate)
        {
            if (forEachItemDelegate is null)
                throw new ArgumentNullException(nameof(forEachItemDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Item == null)
                    continue;

                forEachItemDelegate(tracker.Item, tracker.Owner!, ref tracker.Data);
            }
        }

        /// <summary>
        /// Invokes the specified delegate for each active item in the collection, allowing the caller to process or
        /// modify associated data of type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>Only items considered active are processed; inactive items are skipped. The delegate
        /// can modify the associated data, which will be updated in the collection after each invocation.</remarks>
        /// <typeparam name="T">The type of the data associated with each item to be processed by the delegate.</typeparam>
        /// <param name="forEachItemDelegate">A delegate that is called for each active item, providing the item, its owner, and a reference to its
        /// associated data of type <typeparamref name="T"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachItemDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachActiveItem<T>(ForEachItemDelegate<T> forEachItemDelegate)
        {
            if (forEachItemDelegate is null)
                throw new ArgumentNullException(nameof(forEachItemDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Item == null)
                    continue;

                var dataTemp = tracker.Data is T castData
                    ? castData
                    : default;

                forEachItemDelegate(tracker.Item, tracker.Owner!, ref dataTemp);

                tracker.Data = dataTemp;
            }
        }

        /// <summary>
        /// Invokes the specified delegate for each active pickup, allowing modification of associated pickup data.
        /// </summary>
        /// <remarks>Only pickups with valid trackers and non-null pickup instances are processed.
        /// Modifications to the data parameter within the delegate are applied to the corresponding pickup.</remarks>
        /// <param name="forEachPickupDelegate">A delegate to execute for each active pickup. The delegate receives the pickup instance and a reference to
        /// its associated data, which can be modified.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachPickupDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachActivePickup(ForEachPickupDelegate forEachPickupDelegate)
        {
            if (forEachPickupDelegate is null)
                throw new ArgumentNullException(nameof(forEachPickupDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Pickup == null)
                    continue;

                forEachPickupDelegate(tracker.Pickup, ref tracker.Data);
            }
        }

        /// <summary>
        /// Invokes the specified delegate for each active pickup, allowing modification of associated pickup data.
        /// </summary>
        /// <remarks>Only pickups with valid trackers and non-null pickup instances are processed.
        /// Modifications to the data parameter within the delegate are applied to the corresponding pickup.</remarks>
        /// <param name="forEachPickupDelegate">A delegate to execute for each active pickup. The delegate receives the pickup instance and a reference to
        /// its associated data, which can be modified.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="forEachPickupDelegate"/> is <see langword="null"/>.</exception>
        public void ForEachActivePickup<T>(ForEachPickupDelegate<T> forEachPickupDelegate)
        {
            if (forEachPickupDelegate is null)
                throw new ArgumentNullException(nameof(forEachPickupDelegate));

            for (var x = 0; x < trackers.Count; x++)
            {
                var tracker = trackers[x];

                if (!tracker.ValidateTracker())
                    continue;

                if (tracker.Pickup == null)
                    continue;

                var dataTemp = tracker.Data is T castData
                    ? castData
                    : default;

                forEachPickupDelegate(tracker.Pickup, ref dataTemp);

                tracker.Data = dataTemp;
            }
        }

        /// <summary>
        /// Spawns a new item pickup at the specified position with the given rotation and associates it with optional
        /// pickup data.
        /// </summary>
        /// <param name="position">The world position where the item pickup will be spawned.</param>
        /// <param name="rotation">The rotation to apply to the spawned item pickup. If null, a default rotation is used.</param>
        /// <param name="pickupData">Optional data to associate with the spawned item pickup. The value can be null or omitted if no additional
        /// data is required.</param>
        /// <returns>An instance of the spawned item pickup at the specified position and rotation.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the pickup type is 'None' or if the pickup type represents ammunition, as these cannot be spawned.</exception>
        /// <exception cref="Exception">Thrown if the item pickup instance could not be created.</exception>
        public virtual ItemPickupBase SpawnItem(Vector3 position, Quaternion? rotation, object? pickupData = null)
        {
            if (PickupType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be spawned");

            if (PickupType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            var pickup = PickupType.GetPickupInstance<ItemPickupBase>(position, Scale, rotation, null, true);

            if (pickup == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {PickupType}");

            var eventArgs = new CustomItemSpawnedEventArgs(this, pickup, pickupData);

            OnPickupSpawned(eventArgs);

            Internal_TrackPickup(pickup, eventArgs.PickupData);
            return pickup;
        }

        /// <summary>
        /// Adds this custom item to the specified player's inventory, optionally transferring ownership from an
        /// existing pickup and setting additional item data.
        /// </summary>
        /// <remarks>The method enforces inventory limits and item type restrictions. If the item is set
        /// as held, it becomes the player's active item. Ownership and tracking are updated according to whether the
        /// pickup is destroyed or retained.</remarks>
        /// <param name="pickup">The item pickup to transfer to the target player. Must belong to this custom item.</param>
        /// <param name="target">The player who will receive the item. Cannot be null and must have a valid reference hub.</param>
        /// <param name="newData">Optional. Additional data to associate with the item upon transfer. If null, uses the pickup's existing data
        /// when not destroying the pickup.</param>
        /// <param name="setHeld">If <see langword="true"/>, sets the given item as the player's currently held item after adding it.</param>
        /// <param name="destroyPickup">If <see langword="true"/>, destroys the pickup after transferring the item; otherwise, the pickup remains
        /// and its data is reused if <paramref name="newData"/> is null.</param>
        /// <returns>An instance of <see cref="ItemBase"/> representing the item added to the player's inventory.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null or does not have a valid reference hub.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the item type is None, is ammo, or if the target player's inventory is full.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="pickup"/> does not belong to this custom item.</exception>
        /// <exception cref="Exception">Thrown if the item instance could not be created for the specified item type.</exception>
        public virtual ItemBase GiveItem(ItemPickupBase pickup, ExPlayer target, object? newData = null, bool setHeld = false, bool destroyPickup = true)
        {
            if (target?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(target));

            if (InventoryType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be added to inventory");

            if (InventoryType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            if (target.Inventory.ItemCount >= 8)
                throw new InvalidOperationException($"[{Name} - {Id}] The target player's inventory is full.");

            if (!Internal_CheckItem(pickup.Info.Serial, out var tracker))
                throw new ArgumentException($"[{Name} - {Id}] The specified pickup does not belong to this custom item.", nameof(pickup));

            var item = InventoryType.GetItemInstance<ItemBase>(destroyPickup ? pickup.Info.Serial : null);

            if (item == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {InventoryType}");

            item.TransferItem(target.ReferenceHub);

            var eventArgs = new CustomItemAddedEventArgs(target, this, CustomItemAddReason.PickedUp, item,
                newData != null ? newData
                                : (destroyPickup ? null
                                                : tracker.Data), pickup, tracker.Data);

            OnItemAdded(eventArgs);

            target.Inventory.ownedCustomItems[item.ItemSerial] = this;

            if (destroyPickup)
            {
                pickup.DestroySelf();

                tracker.Pickup = null;

                tracker.Item = item;
                tracker.Owner = target;

                tracker.Data = eventArgs.AddedData;
            }
            else
            {
                trackers.Remove(tracker);

                Internal_TrackItem(item, target, eventArgs.AddedData);
            }

            if (setHeld && target.Inventory.Select(item))
                target.Inventory.CurrentCustomItem = this;

            return item;
        }

        /// <summary>
        /// Adds a new item of the specified type to the target player's inventory and optionally sets it as the
        /// currently held item.
        /// </summary>
        /// <remarks>The method enforces inventory limits and item type restrictions. Only non-ammo,
        /// non-<c>None</c> items can be added, and the target inventory must have space available. Custom event
        /// handlers are invoked after the item is added.</remarks>
        /// <param name="target">The player to whom the item will be added. Cannot be null and must have a valid reference hub.</param>
        /// <param name="itemData">Optional custom data associated with the item. This data is passed to event handlers and may be used for
        /// item customization.</param>
        /// <param name="setHeld">If <see langword="true"/>, the newly added item will be set as the player's currently held item; otherwise,
        /// the held item will not change.</param>
        /// <returns>An instance of <see cref="ItemBase"/> representing the item that was added to the player's inventory.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="target"/> is null or does not have a valid reference hub.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the item type is <c>None</c>, if the item type is ammo, or if the target player's inventory is
        /// full.</exception>
        /// <exception cref="Exception">Thrown if the item instance could not be created for the specified item type.</exception>
        public virtual ItemBase AddItem(ExPlayer target, object? itemData = null, bool setHeld = false)
        {
            if (target?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(target));

            if (InventoryType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be added to inventory");

            if (InventoryType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            if (target.Inventory.ItemCount >= 8)
                throw new InvalidOperationException($"[{Name} - {Id}] The target player's inventory is full.");

            var item = InventoryType.GetItemInstance<ItemBase>();

            if (item == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {InventoryType}");

            item.TransferItem(target.ReferenceHub);

            var eventArgs = new CustomItemAddedEventArgs(target, this, CustomItemAddReason.Added, item, itemData, null, null);

            OnItemAdded(eventArgs);

            Internal_TrackItem(item, target, eventArgs.AddedData);

            target.Inventory.ownedCustomItems[item.ItemSerial] = this;

            if (setHeld && target.Inventory.Select(item))
                target.Inventory.CurrentCustomItem = this;

            return item;
        }

        /// <summary>
        /// Creates a dummy item instance without adding it to any player's inventory.
        /// </summary>
        /// <param name="itemData">Optional data associated with the item. If not specified, the default value for the type is used.</param>
        /// <returns>An instance of the added item. The returned object represents the item.</returns>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="Exception"></exception>
        public virtual ItemBase CreateItem(object? itemData = default)
        {
            if (InventoryType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be added to inventory");

            if (InventoryType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            var item = InventoryType.GetItemInstance<ItemBase>();

            if (item == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {InventoryType}");

            Internal_TrackItem(item, null!, itemData);
            return item;
        }

        /// <summary>
        /// Transfers the specified item to a new owner, optionally setting it as the currently held item for the
        /// recipient.
        /// </summary>
        /// <param name="item">The item to transfer. Cannot be null.</param>
        /// <param name="newOwner">The player who will become the new owner of the item. Cannot be null and must have a valid reference hub.</param>
        /// <param name="setHeld">If <see langword="true"/>, attempts to set the transferred item as the currently held item for the new
        /// owner.</param>
        /// <returns>Returns <see langword="true"/> if the item was successfully transferred; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null or if <paramref name="newOwner"/> is null or does not have a valid
        /// reference hub.</exception>
        public virtual bool TransferItem(ItemBase item, ExPlayer newOwner, bool setHeld = false)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (newOwner?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(newOwner));

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                return false;

            if (ExPlayer.TryGet(item.Owner, out var curOwner))
            {
                if (curOwner.Inventory.CurrentCustomItem != null
                    && curOwner.Inventory.CurrentCustomItem == this
                    && curOwner.Inventory.CurrentItemIdentifier.SerialNumber == item.ItemSerial)
                    curOwner.Inventory.CurrentCustomItem = null;

                curOwner.Inventory.ownedCustomItems.Remove(item.ItemSerial);
            }

            var eventArgs = new CustomItemAddedEventArgs(newOwner, this, CustomItemAddReason.Transferred, item, tracker.Data, null, tracker.Data);

            OnItemAdded(eventArgs);

            item.TransferItem(newOwner.ReferenceHub);

            tracker.Owner = newOwner;
            tracker.Data = eventArgs.AddedData;

            newOwner.Inventory.ownedCustomItems[item.ItemSerial] = this;

            if (setHeld && newOwner.Inventory.Select(item))
                newOwner.Inventory.CurrentCustomItem = this;

            return true;
        }

        /// <summary>
        /// Drops the specified item from the inventory and spawns its pickup.
        /// </summary>
        /// <param name="item">The item to be dropped. Must belong to this custom item and have a valid owner.</param>
        /// <param name="simulateEffects">Whether or not to simulate the item being dropped (this cancels or activates some effects depending on the item's type).</param>
        /// <returns>An instance of the spawned item pickup representing the dropped item.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="item"/> does not belong to this custom item.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the pickup type is <see cref="ItemType.None"/>, if the pickup type is ammo, or if the item does
        /// not have a valid owner.</exception>
        /// <exception cref="Exception">Thrown if the pickup instance could not be created.</exception>
        public virtual ItemPickupBase DropItem(ItemBase item, bool simulateEffects = true)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                throw new ArgumentException($"[{Name} - {Id}] The specified item does not belong to this custom item.", nameof(item));

            if (PickupType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be spawned");

            if (PickupType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            if (!ExPlayer.TryGet(item.Owner, out var curOwner))
                throw new InvalidOperationException($"[{Name} - {Id}] The item does not have a valid owner.");

            var pickup = PickupType.GetPickupInstance<ItemPickupBase>(curOwner.Position, Scale, curOwner.Rotation, item.ItemSerial, true);

            if (pickup == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {PickupType}");

            var eventArgs = new CustomItemDroppedEventArgs(this, curOwner, item, pickup, tracker.Data, false);

            if (curOwner.Inventory.CurrentCustomItem != null
                && curOwner.Inventory.CurrentCustomItem == this
                && curOwner.Inventory.CurrentItemIdentifier.SerialNumber == item.ItemSerial)
                curOwner.Inventory.CurrentCustomItem = null;

            curOwner.Inventory.ownedCustomItems.Remove(item.ItemSerial);

            OnItemDropped(eventArgs);

            if (simulateEffects)
                item.SimulateDrop(out _);

            item.DestroyItem();

            tracker.Item = null;
            tracker.Owner = null;

            tracker.Pickup = pickup;
            tracker.Data = eventArgs.PickupData;

            return pickup;
        }

        /// <summary>
        /// Throws the specified item from the owner's inventory, creating a pickup with the given
        /// force.
        /// </summary>
        /// <remarks>The thrown item is removed from the owner's inventory after the pickup is created.
        /// This method does not support throwing items of type None or ammo.</remarks>
        /// <param name="item">The item to be thrown. Must belong to this custom item and have a valid owner.</param>
        /// <param name="throwForce">The force with which to throw the item. Must be a positive value. The default is 1.0.</param>
        /// <returns>An instance of ItemPickupBase representing the newly created pickup in the game world.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the item parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified item does not belong to this custom item.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the pickup type is None, if the pickup type is ammo, or if the item does not have a valid owner.</exception>
        /// <exception cref="Exception">Thrown if the pickup instance could not be created.</exception>
        public virtual ItemPickupBase ThrowItem(ItemBase item, float throwForce = 1f)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                throw new ArgumentException($"[{Name} - {Id}] The specified item does not belong to this custom item.", nameof(item));

            if (PickupType is ItemType.None)
                throw new InvalidOperationException($"[{Name} - {Id}] None cannot be spawned");

            if (PickupType.IsAmmo())
                throw new InvalidOperationException($"[{Name} - {Id}] Ammo is not supported");

            if (!ExPlayer.TryGet(item.Owner, out var curOwner))
                throw new InvalidOperationException($"[{Name} - {Id}] The item does not have a valid owner.");

            var pickup = curOwner.Inventory.ThrowItem<ItemPickupBase>(PickupType, throwForce, Scale, item.ItemSerial);

            if (pickup == null)
                throw new Exception($"[{Name} - {Id}] Failed to create an instance of {PickupType}");

            if (curOwner.Inventory.CurrentCustomItem != null
                && curOwner.Inventory.CurrentCustomItem == this
                && curOwner.Inventory.CurrentItemIdentifier.SerialNumber == item.ItemSerial)
                curOwner.Inventory.CurrentCustomItem = null;

            curOwner.Inventory.ownedCustomItems.Remove(item.ItemSerial);

            var eventArgs = new CustomItemDroppedEventArgs(this, curOwner, item, pickup, tracker.Data, true);

            OnItemDropped(eventArgs);

            item.DestroyItem();

            tracker.Item = null;
            tracker.Owner = null;

            tracker.Pickup = pickup;
            tracker.Data = eventArgs.PickupData;

            return pickup;
        }

        /// <summary>
        /// Attempts to destroy the item with the specified serial number from the inventory or pickups.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to be destroyed. Must correspond to an existing item in the inventory or
        /// pickups.</param>
        /// <returns>true if the item was found and successfully destroyed; otherwise, false.</returns>
        public virtual bool DestroyItem(ushort itemSerial)
        {
            if (InventoryExtensions.ServerTryGetItemWithSerial(itemSerial, out var item))
                return DestroyItem(item);

            if (ExMap.Pickups.TryGetFirst(x => x.Info.Serial == itemSerial, out var pickup))
                return DestroyItem(pickup);

            return false;
        }

        /// <summary>
        /// Destroys the specified item and removes it from the collection.
        /// </summary>
        /// <remarks>If the item does not exist in the collection, the method returns false and no action
        /// is taken. When an item is destroyed, the <c>OnItemDestroyed</c> event is raised before the item's own
        /// destruction logic is executed.</remarks>
        /// <param name="item">The item to be destroyed. Cannot be null.</param>
        /// <returns>true if the item was successfully destroyed and removed; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is null.</exception>
        public virtual bool DestroyItem(ItemBase item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                return false;

            return DestroyTracker(tracker);
        }

        /// <summary>
        /// Destroys the specified item pickup and removes it from the collection.
        /// </summary>
        /// <remarks>If the specified item pickup does not exist in the collection, the method returns
        /// false and no action is taken. Otherwise, the item is removed and any associated destruction events are
        /// triggered.</remarks>
        /// <param name="pickup">The item pickup to be destroyed. Cannot be null.</param>
        /// <returns>true if the item pickup was successfully destroyed; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="pickup"/> is null.</exception>
        public virtual bool DestroyItem(ItemPickupBase pickup)
        {
            if (pickup == null)
                throw new ArgumentNullException(nameof(pickup));

            if (!Internal_CheckItem(pickup.Info.Serial, out var tracker))
                return false;

            return DestroyTracker(tracker);
        }

        /// <summary>
        /// Destroys an active item tracker and it's tracked items.
        /// </summary>
        /// <param name="tracker">The tracker to destroy.</param>
        /// <returns>true if the tracker was destroyed</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool DestroyTracker(TrackedCustomItem tracker)
        {
            if (tracker is null)
                throw new ArgumentNullException(nameof(tracker));

            if (trackers.Remove(tracker))
            {
                if (tracker.ValidateTracker())
                {
                    if (tracker.Owner?.ReferenceHub != null)
                    {
                        tracker.Owner.Inventory.ownedCustomItems.Remove(tracker.TargetSerial);

                        if (tracker.Owner.Inventory.CurrentCustomItem != null
                            && tracker.Owner.Inventory.CurrentCustomItem == this
                            && tracker.Owner.Inventory.CurrentItemIdentifier.SerialNumber == tracker.TargetSerial)
                            tracker.Owner.Inventory.CurrentCustomItem = null;
                    }

                    if (tracker.Item != null)
                    {
                        OnItemDestroyed(new(this, tracker.TargetSerial, tracker.Item, null, tracker.Data));

                        tracker.Item.DestroyItem();
                    }

                    if (tracker.Pickup != null)
                    {
                        OnPickupDestroyed(new(this, tracker.TargetSerial, null, tracker.Pickup, tracker.Data));

                        tracker.Pickup.DestroySelf();
                    }
                }

                tracker.Data = null;
                tracker.Item = null;
                tracker.Owner = null;
                tracker.Pickup = null;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Destroys all tracked instances and returns the number of instances that were successfully destroyed.
        /// </summary>
        /// <returns>The number of instances that were destroyed. Returns 0 if no instances were destroyed.</returns>
        public int DestroyInstances()
        {
            var count = 0;

            foreach (var tracker in trackers.ToList())
            {
                if (DestroyTracker(tracker))
                {
                    count++;
                }
            }

            trackers.Clear();
            return count;
        }

        #region Event Callbacks
        /// <summary>
        /// Handles the event triggered when a player throws an item, allowing modification of the item's data before it
        /// is processed.
        /// </summary>
        /// <remarks>Modifying <paramref name="itemData"/> within this method affects how the item is
        /// handled after being thrown. This method is typically used to customize item behavior or apply additional
        /// logic when items are thrown by players.</remarks>
        /// <param name="args">The event data associated with the player throwing the item, containing information about the player and the
        /// item being thrown. Cannot be null.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnThrowingItem(LabExtended.Events.Player.PlayerThrowingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles logic when a player throws an item, allowing modification of the item data before it is processed.
        /// </summary>
        /// <param name="args">The event data associated with the player throwing the item, including information about the player and the
        /// thrown item.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnThrewItem(PlayerThrewItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called before a player starts using this custom item.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnUsingItem(PlayerUsingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called after a player starts using this custom item.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnUsedItem(PlayerUsedItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called before a player stops using this custom item.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnCancellingUsingItem(PlayerCancellingUsingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called after a player stops using this custom item.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnCancelledUsingItem(PlayerCancelledUsingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event that occurs when a player attempts to drop an item, allowing modification or cancellation
        /// of the drop operation.
        /// </summary>
        /// <param name="args">The event data containing information about the player and the item being dropped.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnDroppingItem(PlayerDroppingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called before a player picks up this custom item.
        /// </summary>
        /// <remarks>Finished pickup is handled via <see cref="OnItemAdded(CustomItemAddedEventArgs)"/></remarks>
        /// <param name="args">The event's arguments.</param>
        /// <param name="pickupData">A reference to the pickup's custom data.</param>
        public virtual void OnPickingUp(PlayerPickingUpItemEventArgs args, ref object? pickupData)
        {

        }

        /// <summary>
        /// Gets called before a player unselects this custom item.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        /// <param name="itemData">Custom item properties.</param>
        public virtual void OnUnselecting(PlayerSelectingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called after a player unselects this custom item.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        /// <param name="itemData">Custom item properties.</param>
        public virtual void OnUnselected(PlayerSelectedItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called before a player selects this custom item.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        /// <param name="itemData">Custom item properties.</param>
        public virtual void OnSelecting(PlayerSelectingItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called after a player selects this custom item.
        /// </summary>
        /// <param name="args">Event arguments.</param>
        /// <param name="itemData">Custom item properties.</param>
        public virtual void OnSelected(PlayerSelectedItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when a player toggles a flashlight.
        /// </summary>
        /// <param name="args">The event data associated with the player's flashlight toggle action. Contains information about the player
        /// and the toggle state.</param>
        /// <param name="itemData">A reference to the item data related to the flashlight.</param>
        public virtual void OnTogglingLight(PlayerTogglingFlashlightEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when a player toggles their flashlight.
        /// </summary>
        /// <param name="args">The event data associated with the player's flashlight toggle action, including information about the player
        /// and the flashlight state.</param>
        /// <param name="itemData">A reference to the item-specific data./param>
        public virtual void OnToggledLight(PlayerToggledFlashlightEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event that occurs when a player flips a coin.
        /// </summary>
        /// <param name="args">The event data containing information about the player and the coin flip action.</param>
        /// <param name="itemData">A reference to an object representing item-specific data.</param>
        public virtual void OnFlippingCoin(PlayerFlippingCoinEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when a player flips a coin.
        /// </summary>
        /// <param name="args">The event data containing information about the player and the coin flip action. Cannot be null.</param>
        /// <param name="itemData">A reference to the item's data..</param>
        public virtual void OnFlippedCoin(PlayerFlippedCoinEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event that occurs when an item is being upgraded in SCP-914, allowing customization of the
        /// upgrade process.
        /// </summary>
        /// <param name="args">The event data containing information about the item being processed and the context of the upgrade
        /// operation.</param>
        /// <param name="itemData">A reference to an object representing the item's data.</param>
        public virtual void OnUpgradingItem(Scp914ProcessingInventoryItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when an inventory item is upgraded by SCP-914.
        /// </summary>
        /// <param name="args">The event arguments containing information about the processed inventory item, including details of the
        /// upgrade operation.</param>
        /// <param name="itemData">A reference to the data associated with the upgraded item.</param>
        public virtual void OnUpgradedItem(Scp914ProcessedInventoryItemEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when a pickup item is processed by SCP-914 during an upgrade operation.
        /// </summary>
        /// <param name="args">The event data containing information about the pickup item being processed and the context of the SCP-914
        /// upgrade.</param>
        /// <param name="itemData">A reference to an object that holds custom data associated with the item.</param>
        public virtual void OnUpgradingPickup(Scp914ProcessingPickupEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles the event triggered when a pickup item is processed by SCP-914 and upgraded.
        /// </summary>
        /// <param name="args">The event data containing information about the processed pickup item and the SCP-914 upgrade operation.</param>
        /// <param name="itemData">A reference to an object containing custom data associated with the item.</param>
        public virtual void OnUpgradedPickup(Scp914ProcessedPickupEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles logic that occurs when a player is being disarmed.
        /// </summary>
        /// <param name="args">The event data containing information about the player being disarmed and the context of the disarming
        /// action.</param>
        /// <param name="itemData">A reference to the item's data./param>
        public virtual void OnDisarming(PlayerCuffingEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Handles logic when a player is disarmed.
        /// </summary>
        /// <param name="args">The event data containing information about the player who was disarmed.</param>
        /// <param name="itemData">A reference to the item data associated with the disarm event..</param>
        public virtual void OnDisarmed(PlayerCuffedEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets call before a player escapes.
        /// </summary>
        /// <remarks>This event is triggered for <b>every</b> custom item instance found in the player's inventory!</remarks>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnEscaping(PlayerEscapingEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets call after a player escapes.
        /// </summary>
        /// <remarks>This event is triggered for <b>every</b> custom item instance found in the player's inventory!</remarks>
        /// <param name="args">The event's arguments.</param>
        /// <param name="itemData">A reference to the item's custom data.</param>
        public virtual void OnEscaped(PlayerEscapedEventArgs args, ref object? itemData)
        {

        }

        /// <summary>
        /// Gets called when a custom item's pickup collides with another collider.
        /// </summary>
        /// <param name="args">The event's arguments.</param>
        /// <param name="pickupData">A reference to the pickup's custom data.</param>
        public virtual void OnCollided(PickupCollidedEventArgs args, ref object? pickupData)
        {

        }
        #endregion

        #region Custom Item specific event callbacks
        /// <summary>
        /// Gets called once the item has been registered.
        /// </summary>
        public virtual void OnRegistered() { }

        /// <summary>
        /// Gets called once the item has been unregistered.
        /// </summary>
        public virtual void OnUnregistered() { }

        /// <summary>
        /// Called when an item is added to a player -or- when the player picks up a spawned pickup.
        /// </summary>
        public virtual void OnItemAdded(CustomItemAddedEventArgs args)
        {
            ItemAdded?.InvokeSafe(args);
        }

        /// <summary>
        /// Called when the item is destroyed to perform any necessary cleanup or finalization.
        /// </summary>
        public virtual void OnItemDestroyed(CustomItemDestroyedEventArgs args)
        {
            ItemDestroyed?.InvokeSafe(args);
        }

        /// <summary>
        /// Called when an item is dropped by a player. Allows custom handling of the item drop event.
        /// </summary>
        public virtual void OnItemDropped(CustomItemDroppedEventArgs args)
        {
            ItemDropped?.InvokeSafe(args);
        }

        /// <summary>
        /// Handles logic to be executed when an item pickup is destroyed.
        /// </summary>
        public virtual void OnPickupDestroyed(CustomItemDestroyedEventArgs args)
        {
            ItemDestroyed?.InvokeSafe(args);
        }

        /// <summary>
        /// Called when an item is spawned. Allows custom logic to be executed when the pickup and its
        /// associated item data are initialized.
        /// </summary>
        public virtual void OnPickupSpawned(CustomItemSpawnedEventArgs args)
        {
            ItemSpawned?.InvokeSafe(args);
        }
        #endregion

        #region CheckItem
        /// <summary>
        /// Determines whether an item with the specified serial number exists in the collection.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <returns>true if an item with the specified serial number exists in the collection; otherwise, false.</returns>
        public bool CheckItem(ushort itemSerial)
            => trackers.Any(x => x.TargetSerial == itemSerial && x.ValidateTracker());

        /// <summary>
        /// Determines whether an item with the specified serial number exists in the collection and retrieves its
        /// associated data.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item serial number, if the item is
        /// found; otherwise, the default value for the type of the item data. This parameter is passed uninitialized.</param>
        /// <returns>true if the collection contains an item with the specified serial number; otherwise, false.</returns>
        public bool CheckItem(ushort itemSerial, out object itemData)
        {
            if (!Internal_CheckItem(itemSerial, out var tracker))
            {
                itemData = default!;
                return false;
            }

            itemData = tracker.Data!;
            return true;
        }

        /// <summary>
        /// Determines whether an item with the specified serial number exists in the collection and retrieves its
        /// associated data.
        /// </summary>
        /// <param name="itemSerial">The serial number of the item to locate.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item serial number, if the item is
        /// found; otherwise, the default value for the type of the item data. This parameter is passed uninitialized.</param>
        /// <returns>true if the collection contains an item with the specified serial number; otherwise, false.</returns>
        public bool CheckItem<T>(ushort itemSerial, out T itemData)
        {
            itemData = default!;

            if (!Internal_CheckItem(itemSerial, out var tracker))
                return false;

            if (tracker.Data is T castedData)
                itemData = castedData;

            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection and has a valid base definition.
        /// </summary>
        /// <param name="item">The item to check for existence and validity. Cannot be null.</param>
        /// <returns>true if the item exists in the collection and its base definition is not null; otherwise, false.</returns>
        public bool CheckItem(Item item)
            => item?.Base != null && trackers.Any(x => x.TargetSerial == item.Serial && x.ValidateTracker());

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null and must have a non-null Base property.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for the type of the data parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem(Item item, out object itemData)
        {
            itemData = default!;

            if (item?.Base == null)
                return false;

            if (!Internal_CheckItem(item.Serial, out var tracker))
                return false;

            itemData = tracker.Data!;
            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null and must have a non-null Base property.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for the type of the data parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem<T>(Item item, out T itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.Serial, out var tracker))
                return false;

            if (tracker.Data is T castedData)
                itemData = castedData;

            return true;
        }

        /// <summary>
        /// Determines whether the specified pickup item exists in the collection.
        /// </summary>
        /// <param name="item">The pickup item to check for existence. Cannot be null.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem(Pickup item)
            => item?.Base != null && trackers.Any(x => x.TargetSerial == item.Serial && x.ValidateTracker());

        /// <summary>
        /// Determines whether the specified pickup corresponds to a known item and retrieves its associated data.
        /// </summary>
        /// <param name="item">The pickup to check for a corresponding item. Cannot be null; the item's Base property must also not be
        /// null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the item if found; otherwise, the default value
        /// for type T. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the specified pickup matches a known item and its data is retrieved; otherwise,
        /// <see langword="false"/>.</returns>
        public bool CheckItem(Pickup item, out object itemData)
        {
            itemData = default!;

            if (item?.Base == null)
                return false;

            if (!Internal_CheckItem(item.Serial, out var tracker))
                return false;

            itemData = tracker.Data!;
            return true;
        }

        /// <summary>
        /// Determines whether the specified pickup corresponds to a known item and retrieves its associated data.
        /// </summary>
        /// <param name="item">The pickup to check for a corresponding item. Cannot be null; the item's Base property must also not be
        /// null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the item if found; otherwise, the default value
        /// for type T. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the specified pickup matches a known item and its data is retrieved; otherwise,
        /// <see langword="false"/>.</returns>
        public bool CheckItem<T>(Pickup item, out T itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.Serial, out var tracker))
                return false;

            if (tracker.Data is T castedData)
                itemData = castedData;

            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem(ItemBase item)
            => item != null && trackers.Any(x => x.TargetSerial == item.ItemSerial && x.ValidateTracker());

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for type T.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem(ItemBase item, out object itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                return false;

            itemData = tracker.Data!;
            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for type T.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem<T>(ItemBase item, out T itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.ItemSerial, out var tracker))
                return false;

            if (tracker.Data is T castedData)
                itemData = castedData;

            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <returns>true if the specified item is found in the collection; otherwise, false.</returns>
        public bool CheckItem(ItemPickupBase item)
            => item != null && trackers.Any(x => x.TargetSerial == item.Info.Serial && x.ValidateTracker());

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for the type of the data parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem(ItemPickupBase item, out object itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.Info.Serial, out var tracker))
                return false;

            itemData = tracker.Data!;
            return true;
        }

        /// <summary>
        /// Determines whether the specified item exists in the collection and retrieves its associated data if found.
        /// </summary>
        /// <param name="item">The item to locate in the collection. Cannot be null.</param>
        /// <param name="itemData">When this method returns, contains the data associated with the specified item if it is found; otherwise,
        /// the default value for the type of the data parameter. This parameter is passed uninitialized.</param>
        /// <returns>true if the item exists in the collection; otherwise, false.</returns>
        public bool CheckItem<T>(ItemPickupBase item, out T itemData)
        {
            itemData = default!;

            if (item == null)
                return false;

            if (!Internal_CheckItem(item.Info.Serial, out var tracker))
                return false;

            if (tracker.Data is T castedData)
                itemData = castedData;

            return true;
        }
        #endregion

        #region Internal
        internal bool Internal_CheckItem(ushort itemSerial, out TrackedCustomItem trackedCustomItem)
        {
            var invalidCount = 0;

            for (var i = 0; i < trackers.Count; i++)
            {
                var tracker = trackers[i];

                if (tracker.TargetSerial == itemSerial)
                {
                    if (!tracker.ValidateTracker())
                    {
                        invalidCount++;
                        continue;
                    }

                    if (invalidCount > 0)
                        trackers.RemoveAll(x => !x.ValidateTracker());

                    trackedCustomItem = tracker;
                    return true;
                }
            }

            if (invalidCount > 0)
                trackers.RemoveAll(x => !x.ValidateTracker());

            trackedCustomItem = null!;
            return false;
        }

        internal void Internal_TrackPickup(ItemPickupBase pickup, object? pickupData)
        {
            trackers.Add(new TrackedCustomItem(this, pickup.Info.Serial, null, null, pickup, pickupData));
        }

        internal void Internal_TrackItem(ItemBase item, ExPlayer owner, object? itemData)
        {
            trackers.Add(new TrackedCustomItem(this, item.ItemSerial, owner, item, null, itemData));
        }

        internal static bool Internal_GetCustomItem(ushort itemSerial, out CustomItem customItem, out TrackedCustomItem itemTracker)
        {
            customItem = null!;
            itemTracker = null!;

            foreach (var pair in itemsById)
            {
                if (pair.Value.Internal_CheckItem(itemSerial, out var tracker))
                {
                    customItem = pair.Value;
                    itemTracker = tracker;

                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Static Event Handling
        internal static void Internal_Init()
        {
            // is this what adhd is??
            PlayerEvents.PickingUpItem += Internal_PickingUpItem;
            PlayerEvents.PickedUpItem += Internal_PickedUp;

            PlayerEvents.TogglingFlashlight += Internal_TogglingFlashlight;
            PlayerEvents.ToggledFlashlight += Internal_ToggledFlashlight;

            PlayerEvents.UsingItem += Internal_UsingItem;
            PlayerEvents.UsedItem += Internal_UsedItem;

            PlayerEvents.CancellingUsingItem += Internal_CancellingUsingItem;
            PlayerEvents.CancelledUsingItem += Internal_CancelledUsingItem;

            PlayerEvents.FlippingCoin += Internal_FlippingCoin;
            PlayerEvents.FlippedCoin += Internal_FlippedCoin;

            PlayerEvents.Cuffing += Internal_Cuffing;
            PlayerEvents.Cuffed += Internal_Cuffed;

            PlayerEvents.Escaping += Internal_Escaping;
            PlayerEvents.Escaped += Internal_Escaped;

            Scp914Events.ProcessingInventoryItem += Internal_UpgradingItem;
            Scp914Events.ProcessedInventoryItem += Internal_UpgradedItem;

            Scp914Events.ProcessingPickup += Internal_UpgradingPickup;
            Scp914Events.ProcessedPickup += Internal_UpgradedPickup;

            ExPlayerEvents.SelectingItem += Internal_Selecting;
            ExPlayerEvents.SelectedItem += Internal_Selected;

            ExPlayerEvents.Leaving += Internal_Leaving;
            ExMapEvents.PickupCollided += Internal_Collided;
            ExRoundEvents.WaitingForPlayers += Internal_Waiting;
        }

        private static void Internal_Leaving(PlayerLeavingEventArgs args)
        {
            if (args.Player.Inventory.ItemCount > 0)
            {
                foreach (var item in args.Player.Inventory.Items.ToList())
                {
                    if (IsTrackedItem(item.ItemSerial, out var itemTracker))
                    {
                        if (itemTracker.TargetItem.DropOnOwnerLeave)
                        {
                            itemTracker.TargetItem.DropItem(item, false);
                        }
                        else
                        {
                            itemTracker.TargetItem.DestroyItem(item);
                        }
                    }
                }
            }

            if (args.Player.Inventory.droppedItems?.Count > 0)
            {
                foreach (var pickup in args.Player.Inventory.droppedItems.ToList())
                {
                    if (pickup != null && IsTrackedItem(pickup.Info.Serial, out var itemTracker))
                    {
                        if (itemTracker.TargetItem.DestroyOnOwnerLeave)
                        {
                            itemTracker.TargetItem.DestroyItem(pickup);
                        }
                    }
                }
            }
        }

        private static void Internal_PickingUpItem(PlayerPickingUpItemEventArgs args)
        {
            if (args.Player is ExPlayer player)
            {
                if (args.Pickup?.Base != null)
                {
                    if (Internal_GetCustomItem(args.Pickup.Serial, out var customItem, out var tracker))
                    {
                        customItem.OnPickingUp(args, ref tracker.Data);
                    }
                }
            }
        }

        private static void Internal_Collided(PickupCollidedEventArgs args)
        {
            if (args.Pickup != null)
            {
                if (Internal_GetCustomItem(args.Pickup.Info.Serial, out var customItem, out var tracker))
                {
                    customItem.OnCollided(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_PickedUp(PlayerPickedUpItemEventArgs args)
        {
            if (args.Item?.Base == null)
                return;

            if (args.Player is not ExPlayer player)
                return;

            if (!Internal_GetCustomItem(args.Item.Serial, out var customItem, out var tracker))
                return;

            if (customItem.InventoryType == args.Item.Type)
            {
                var eventArgs = new CustomItemAddedEventArgs(player, customItem, CustomItemAddReason.PickedUp, args.Item.Base, tracker.Data, tracker.Pickup, tracker.Data);

                customItem.OnItemAdded(eventArgs);

                tracker.Data = eventArgs.AddedData;

                tracker.Item = args.Item.Base;
                tracker.Owner = player;

                tracker.Pickup = null;
            }
            else
            {
                if (customItem.InventoryType is ItemType.None)
                {
                    ApiLog.Warn("Custom Items", $"&3{customItem.Id}&r has not defined it's inventory type!");
                    return;
                }

                if (customItem.InventoryType.IsAmmo())
                {
                    ApiLog.Warn("Custom Items", $"&3{customItem.Id}&r has defined it's inventory type as ammo, which is not supported!");
                    return;
                }

                var itemInstance = customItem.InventoryType.GetItemInstance<ItemBase>(args.Item.Serial);

                if (itemInstance == null)
                {
                    ApiLog.Warn("Custom Items", $"&3{customItem.Id}&r failed to create an item instance for item &3{args.Item.Serial}&r!");
                    return;
                }

                args.Item.Base.DestroyItem();

                itemInstance.TransferItem(player.ReferenceHub);

                var eventArgs = new CustomItemAddedEventArgs(player, customItem, CustomItemAddReason.PickedUp, itemInstance, tracker.Data, tracker.Pickup, tracker.Data);

                customItem.OnItemAdded(eventArgs);

                tracker.Data = eventArgs.AddedData;

                tracker.Item = itemInstance;
                tracker.Owner = player;

                tracker.Pickup = null;
            }
        }

        private static void Internal_TogglingFlashlight(PlayerTogglingFlashlightEventArgs args)
        {
            if (args.LightItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.LightItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnTogglingLight(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_ToggledFlashlight(PlayerToggledFlashlightEventArgs args)
        {
            if (args.LightItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.LightItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnToggledLight(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_FlippingCoin(PlayerFlippingCoinEventArgs args)
        {
            if (args.CoinItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.CoinItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnFlippingCoin(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_FlippedCoin(PlayerFlippedCoinEventArgs args)
        {
            if (args.CoinItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.CoinItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnFlippedCoin(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_UpgradingItem(Scp914ProcessingInventoryItemEventArgs args)
        {
            if (args.Item?.Base != null)
            {
                if (Internal_GetCustomItem(args.Item.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUpgradingItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_UpgradedItem(Scp914ProcessedInventoryItemEventArgs args)
        {
            if (args.Item?.Base != null)
            {
                if (Internal_GetCustomItem(args.Item.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUpgradedItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_UpgradingPickup(Scp914ProcessingPickupEventArgs args)
        {
            if (args.Pickup?.Base != null)
            {
                if (Internal_GetCustomItem(args.Pickup.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUpgradingPickup(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_UpgradedPickup(Scp914ProcessedPickupEventArgs args)
        {
            if (args.Pickup?.Base != null)
            {
                if (Internal_GetCustomItem(args.Pickup.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUpgradedPickup(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_Cuffing(PlayerCuffingEventArgs args)
        {
            if (args.Player is ExPlayer player)
            {
                if (Internal_GetCustomItem(player.Inventory.CurrentItemIdentifier.SerialNumber, out var customItem, out var tracker))
                {
                    customItem.OnDisarming(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_Cuffed(PlayerCuffedEventArgs args)
        {
            if (args.Player is ExPlayer player)
            {
                if (Internal_GetCustomItem(player.Inventory.CurrentItemIdentifier.SerialNumber, out var customItem, out var tracker))
                {
                    customItem.OnDisarmed(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_Escaping(PlayerEscapingEventArgs args)
        {
            if (args.Player is ExPlayer player)
            {
                foreach (var item in player.Inventory.Items)
                {
                    if (Internal_GetCustomItem(item.ItemSerial, out var customItem, out var tracker))
                    {
                        customItem.OnEscaping(args, ref tracker.Data);

                        if (!args.IsAllowed)
                            break;
                    }
                }
            }
        }

        private static void Internal_Escaped(PlayerEscapedEventArgs args)
        {
            if (args.Player is ExPlayer player)
            {
                foreach (var item in player.Inventory.Items)
                {
                    if (Internal_GetCustomItem(item.ItemSerial, out var customItem, out var tracker))
                    {
                        customItem.OnEscaped(args, ref tracker.Data);
                    }
                }
            }
        }

        private static void Internal_Selecting(PlayerSelectingItemEventArgs args)
        {
            if (args.CurrentItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.CurrentItem.Serial, out var heldCustomItem, out var heldTracker))
                {
                    heldCustomItem.OnUnselecting(args, ref heldTracker.Data);
                }
            }

            if (args.NextItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.NextItem.Serial, out var nextCustomItem, out var nextTracker))
                {
                    nextCustomItem.OnSelecting(args, ref nextTracker.Data);
                }
            }
        }

        private static void Internal_Selected(PlayerSelectedItemEventArgs args)
        {
            if (args.PreviousItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.PreviousItem.Serial, out var prevCustomItem, out var prevTracker))
                {
                    prevCustomItem.OnUnselected(args, ref prevTracker.Data);

                    if (args.Player.Inventory.CurrentCustomItem != null
                        && args.Player.Inventory.CurrentCustomItem == prevCustomItem)
                        args.Player.Inventory.CurrentCustomItem = null;
                }
            }

            if (args.NewItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.NewItem.Serial, out var newCustomItem, out var newTracker))
                {
                    newCustomItem.OnSelected(args, ref newTracker.Data);

                    args.Player.Inventory.CurrentCustomItem = newCustomItem;
                }
            }
        }

        private static void Internal_UsingItem(PlayerUsingItemEventArgs args)
        {
            if (args.UsableItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.UsableItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUsingItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_UsedItem(PlayerUsedItemEventArgs args)
        {
            if (args.UsableItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.UsableItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnUsedItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_CancellingUsingItem(PlayerCancellingUsingItemEventArgs args)
        {
            if (args.UsableItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.UsableItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnCancellingUsingItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_CancelledUsingItem(PlayerCancelledUsingItemEventArgs args)
        {
            if (args.UsableItem?.Base != null)
            {
                if (Internal_GetCustomItem(args.UsableItem.Serial, out var customItem, out var tracker))
                {
                    customItem.OnCancelledUsingItem(args, ref tracker.Data);
                }
            }
        }

        private static void Internal_Waiting()
        {
            foreach (var item in itemsById)
            {
                item.Value.trackers.Clear();
            }
        }
        #endregion
    }
}