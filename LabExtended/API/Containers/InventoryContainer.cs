﻿using Interactables.Interobjects.DoorUtils;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp330;

using LabExtended.Core.Hooking;
using LabExtended.Events.Player;
using LabExtended.Extensions;

using LabExtended.API.Collections.Locked;

using PluginAPI.Events;

using UnityEngine;
using NorthwoodLib.Pools;
using PlayerRoles.FirstPersonControl;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class used to manage player inventories.
    /// </summary>
    public class InventoryContainer
    {
        internal readonly LockedHashSet<ItemPickupBase> _droppedItems = new LockedHashSet<ItemPickupBase>();

        /// <summary>
        /// Creates a new <see cref="InventoryContainer"/> instance.
        /// </summary>
        /// <param name="inventory">The player <see cref="InventorySystem.Inventory"/> component.</param>
        public InventoryContainer(Inventory inventory)
            => Inventory = inventory;

        /// <summary>
        /// Gets the targeted player's <see cref="InventorySystem.Inventory"/> component.
        /// </summary>
        public Inventory Inventory { get; }

        /// <summary>
        /// Gets the inventory item holder.
        /// </summary>
        public InventoryInfo UserInventory => Inventory.UserInventory;

        /// <summary>
        /// Gets the amount of items in this player's inventory.
        /// </summary>
        public int ItemCount => Inventory.UserInventory.Items.Count;

        /// <summary>
        /// Whether or not the player has any items.
        /// </summary>
        public bool HasAnyItems => Inventory.UserInventory.Items.Count > 0;

        /// <summary>
        /// Gets the player's SCP-330 candy bag (or <see langword="null"/> if the player doesn't have one).
        /// </summary>
        public Scp330Bag CandyBag => Scp330Bag.TryGetBag(Inventory._hub, out var bag) ? bag : null;

        /// <summary>
        /// Gets a list of all items.
        /// </summary>
        public IEnumerable<ItemBase> Items => UserInventory.Items.Values;

        /// <summary>
        /// Gets all <see cref="Firearm"/>s in player's inventory.
        /// </summary>
        public IEnumerable<Firearm> Firearms => Inventory.UserInventory.Items.Values.Where<Firearm>();

        /// <summary>
        /// Gets all <see cref="KeycardItem"/>s in player's inventory.
        /// </summary>
        public IEnumerable<KeycardItem> Keycards => Inventory.UserInventory.Items.Values.Where<KeycardItem>();

        /// <summary>
        /// Gets a list of all items that have been dropped by this player.
        /// </summary>
        public IReadOnlyList<ItemPickupBase> DroppedItems => _droppedItems;

        /// <summary>
        /// Gets permissions of the currently held keycard. <i>(<see cref="KeycardPermissions.None"/> if the player isn't holding a keycard)</i>.
        /// </summary>
        public KeycardPermissions HeldKeycardPermissions => CurrentItem != null && CurrentItem is KeycardItem keycardItem ? keycardItem.Permissions : KeycardPermissions.None;

        /// <summary>
        /// Gets or sets the currently held item instance.
        /// <para>This property is <b>not</b> synchronized to other players.</para>
        /// </summary>
        public ItemBase CurrentItem
        {
            get => Inventory.CurInstance;
            set => Inventory.CurInstance = value;
        }

        /// <summary>
        /// Gets or sets the currently held item type.
        /// <para>Setting an item creates a new item instance that is <b>not</b> added to the player's inventory.</para>
        /// </summary>
        public ItemType CurrentItemType
        {
            get => CurrentItem?.ItemTypeId ?? ItemType.None;
            set
            {
                if (value is ItemType.None)
                {
                    CurrentItemIdentifier = ItemIdentifier.None;
                    return;
                }

                var instance = value.GetItemInstance<ItemBase>();

                instance.SetupItem(Inventory._hub);

                CurrentItemIdentifier = new ItemIdentifier(instance.ItemTypeId, instance.ItemSerial);
            }
        }

        /// <summary>
        /// Gets or sets the current item's identifier.
        /// </summary>
        public ItemIdentifier CurrentItemIdentifier
        {
            get => Inventory.NetworkCurItem;
            set => Inventory.NetworkCurItem = value;
        }

        /// <summary>
        /// Gets a list of all candies in player's candy bag. (empty <see cref="CandyKindID"/> array if no bag is found).
        /// </summary>
        public IEnumerable<CandyKindID> Candies
        {
            get
            {
                var bag = CandyBag;

                if (bag != null)
                    return bag.Candies;

                return Array.Empty<CandyKindID>();
            }
        }

        /// <summary>
        /// Gets or sets a list of the player's inventory item types.
        /// </summary>
        public IEnumerable<ItemType> ItemTypes
        {
            get => Inventory.UserInventory.Items.Values.Select(p => p.ItemTypeId);
            set => ClearInventory(value);
        }

        /// <summary>
        /// Gets all keycard permissions of this player (including inventory items).
        /// </summary>
        public KeycardPermissions AllKeycardPermissions
        {
            get
            {
                var perms = KeycardPermissions.None;

                foreach (var keycard in Keycards)
                {
                    var cardPerms = keycard.Permissions.GetFlags();

                    foreach (var cardPerm in cardPerms)
                    {
                        if (!perms.HasFlagFast(cardPerm))
                            perms |= cardPerm;
                    }
                }

                return perms;
            }
        }

        /// <summary>
        /// Gets or adds a new SCP-330 candy bag to the player's inventory.
        /// </summary>
        /// <returns>The existing/added <see cref="Scp330Bag"/> instance.</returns>
        public Scp330Bag GetOrAddCandyBag()
        {
            if (Scp330Bag.TryGetBag(Inventory._hub, out var bag))
                return bag;

            return AddItem<Scp330Bag>(ItemType.SCP330);
        }

        /// <summary>
        /// Counts all matching candy types.
        /// </summary>
        /// <param name="candyKind">The candy type to count.</param>
        /// <returns>The amount of matching candies.</returns>
        public int CountCandies(CandyKindID candyKind)
            => Candies.Count(x => x == candyKind);

        /// <summary>
        /// Adds a new item to the player's inventory.
        /// </summary>
        /// <typeparam name="T">Generic type of the item.</typeparam>
        /// <param name="type">Type of the item.</param>
        /// <returns>The item instance as <typeparamref name="T"/>.</returns>
        public T AddItem<T>(ItemType type) where T : ItemBase
            => (T)AddItem(type);

        /// <summary>
        /// Adds a new item to the player's inventory.
        /// </summary>
        /// <param name="type">Type of the item.</param>
        /// <returns>The item instance.</returns>
        public ItemBase AddItem(ItemType type)
           => Inventory.ServerAddItem(type);

        /// <summary>
        /// Drops an item with the specified serial.
        /// </summary>
        /// <param name="serial">The serial to drop.</param>
        /// <returns>The pickup instance if dropped succesfully.</returns>
        public ItemPickupBase DropItem(ushort serial)
            => Inventory.ServerDropItem(serial);

        /// <summary>
        /// Drops the specified item.
        /// </summary>
        /// <param name="item">The item to drop.</param>
        /// <returns>The pickup instance if dropped succesfully.</returns>
        public ItemPickupBase DropItem(ItemBase item)
            => Inventory.ServerDropItem(item.ItemSerial);

        /// <summary>
        /// Drops the currently held item.
        /// </summary>
        /// <returns>The pickup instance if dropped succesfully.</returns>
        public ItemPickupBase DropHeldItem()
            => Inventory.ServerDropItem(CurrentItemIdentifier.SerialNumber);

        /// <summary>
        /// Drops a list of items.
        /// </summary>
        /// <param name="predicate">The item filter.</param>
        /// <returns>A lsit of dropped items.</returns>
        public List<ItemPickupBase> DropItems(Predicate<ItemBase> predicate = null)
        {
            var list = new List<ItemPickupBase>();
            var items = ListPool<ItemBase>.Shared.Rent(Items);

            foreach (var item in items)
            {
                if (predicate != null && !predicate(item))
                    continue;

                var pickup = Inventory.ServerDropItem(item.ItemSerial);

                if (pickup is null)
                    continue;

                list.Add(pickup);
            }

            ListPool<ItemBase>.Shared.Return(items);
            return list;
        }

        /// <summary>
        /// Drops a list of items.
        /// </summary>
        /// <param name="types">Item types to drop.</param>
        /// <returns>A list of dropped items.</returns>
        public List<ItemPickupBase> DropItems(params ItemType[] types)
            => DropItems(item => types.Contains(item.ItemTypeId));

        public IEnumerable<T> DropItems<T>(params ItemType[] types) where T : ItemPickupBase
            => DropItems(item => item.PickupDropModel != null && item.PickupDropModel is T).Where<T>(item => types.Length < 1 || types.Contains(item.Info.ItemId));

        public IEnumerable<ItemBase> GetItems(params ItemType[] types)
            => Items.Where(item => types.Contains(item.ItemTypeId));

        public IEnumerable<T> GetItems<T>() where T : ItemBase
        => Items.Where<T>();

        public IEnumerable<T> GetItems<T>(ItemType type) where T : ItemBase
            => Items.Where<T>(item => item.ItemTypeId == type);

        public bool HasItem(ItemType type)
            => Items.Any(it => it.ItemTypeId == type);

        public bool HasItems(ItemType type, int count)
            => Items.Count(it => it.ItemTypeId == type) >= count;

        public bool HasKeycardPermission(KeycardPermissions keycardPermissions)
            => Keycards.Any(card => card.Permissions.HasFlagFast(keycardPermissions));

        public int CountItems(ItemType type)
            => Items.Count(it => it.ItemTypeId == type);

        public void RemoveItem(ushort serial)
            => Inventory.ServerRemoveItem(serial, null);

        public void RemoveItem(ItemBase item, ItemPickupBase pickup = null)
            => Inventory.ServerRemoveItem(item.ItemSerial, pickup);

        public void RemoveHeldItem()
            => Inventory.ServerRemoveItem(CurrentItemIdentifier.SerialNumber, CurrentItem?.PickupDropModel ?? null);

        public void RemoveItems(Predicate<ItemBase> predicate = null)
        {
            var items = ListPool<ItemBase>.Shared.Rent(Items);

            foreach (var item in items)
            {
                if (predicate != null && !predicate(item))
                    continue;

                Inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            }

            ListPool<ItemBase>.Shared.Return(items);
        }

        public void RemoveItems(ItemType type, int count = -1)
        {
            var items = ListPool<ItemBase>.Shared.Rent(Items);
            var removed = 0;

            foreach (var item in items)
            {
                if (item.ItemTypeId != type)
                    continue;

                if (count > 0 && removed >= count)
                    break;

                removed++;

                Inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            }

            ListPool<ItemBase>.Shared.Return(items);
        }

        public void ClearInventory(IEnumerable<ItemType> newInventory = null)
        {
            while (Inventory.UserInventory.Items.Count > 0)
                Inventory.ServerRemoveItem(Inventory.UserInventory.Items.ElementAt(0).Key, null);

            if (newInventory != null)
            {
                foreach (var item in newInventory)
                {
                    if (item != ItemType.None)
                        AddItem(item);
                }
            }

            Inventory.SendItemsNextFrame = true;
        }

        public bool AddOrSpawnItem(ItemType type)
        {
            if (Inventory.UserInventory.Items.Count > 7)
            {
                if (type.TryGetItemPrefab(out var itemPrefab))
                {
                    Inventory.ServerCreatePickup(itemPrefab, new PickupSyncInfo(type, itemPrefab.Weight));
                    return false;
                }

                return false;
            }

            return Inventory.ServerAddItem(type);
        }

        public T ThrowItem<T>(ItemBase item) where T : ItemPickupBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            Inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            return ThrowItem<T>(item.ItemTypeId, item.ItemSerial);
        }

        public T ThrowItem<T>(ItemType itemType, ushort? itemSerial = null) where T : ItemPickupBase
        {
            var itemPrefab = itemType.GetItemPrefab<ItemBase>();
            var pickupInstance = itemType.GetPickupInstance<T>(null, null, null, itemSerial, true);
            var pickupRigidbody = pickupInstance?.GetRigidbody();

            if (pickupRigidbody is null)
                return null;

            if (!EventManager.ExecuteEvent(new PlayerThrowItemEvent(Inventory._hub, itemPrefab, pickupRigidbody)))
                return pickupInstance;

            var velocity = Inventory._hub.GetVelocity();
            var angular = Vector3.Lerp(itemPrefab.ThrowSettings.RandomTorqueA, itemPrefab.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

            velocity = velocity / 3f + Inventory._hub.PlayerCameraReference.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, pickupRigidbody.mass)) + 0.3f);

            velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
            velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
            velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

            var throwingEv = new PlayerThrowingItemArgs(ExPlayer.Get(Inventory._hub), itemPrefab, pickupInstance, pickupRigidbody, Inventory._hub.PlayerCameraReference.position, velocity, angular);

            if (!HookRunner.RunCancellable(throwingEv, true))
                return pickupInstance;

            pickupRigidbody.position = throwingEv.Position;
            pickupRigidbody.velocity = throwingEv.Velocity;
            pickupRigidbody.angularVelocity = throwingEv.AngularVelocity;

            if (pickupRigidbody.angularVelocity.magnitude > pickupRigidbody.maxAngularVelocity)
                pickupRigidbody.maxAngularVelocity = pickupRigidbody.angularVelocity.magnitude;

            return pickupInstance;
        }
    }
}