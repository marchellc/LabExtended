using Interactables.Interobjects.DoorUtils;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;

using LabExtended.API.CustomItems;
using LabExtended.Extensions;

using UnityEngine;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;

using InventorySystem.Items.Usables;
using LabExtended.Utilities;
using LabExtended.Utilities.Keycards;

namespace LabExtended.API.Containers
{
    /// <summary>
    /// A class used to manage player inventories.
    /// </summary>
    public class InventoryContainer : IDisposable
    {
        internal HashSet<ItemPickupBase> _droppedItems;
        internal CustomItemInstance? heldCustomItem;

        /// <summary>
        /// Creates a new <see cref="InventoryContainer"/> instance.
        /// </summary>
        /// <param name="inventory">The player <see cref="InventorySystem.Inventory"/> component.</param>
        public InventoryContainer(Inventory inventory)
        {
            Inventory = inventory;
            UsableItemsHandler = UsableItemsController.GetHandler(inventory._hub);

            _droppedItems = HashSetPool<ItemPickupBase>.Shared.Rent();
        }

        public ItemBase this[ushort itemSerial]
        {
            get => UserInventory.Items.TryGetValue(itemSerial, out var item) ? item : null;
            set
            {
                UserInventory.Items[itemSerial] = value;
                Inventory.SendItemsNextFrame = true;
            }
        }

        /// <summary>
        /// Gets the targeted player's <see cref="InventorySystem.Inventory"/> component.
        /// </summary>
        public Inventory Inventory { get; }

        /// <summary>
        /// Gets the targeted player's <see cref="PlayerHandler"/> usable item handler.
        /// </summary>
        public PlayerHandler UsableItemsHandler { get; }

        /// <summary>
        /// Gets the inventory item holder.
        /// </summary>
        public InventoryInfo UserInventory => Inventory.UserInventory;
        
        /// <summary>
        /// Gets the currently held Custom Item instance.
        /// </summary>
        public CustomItemInstance? HeldCustomItem => heldCustomItem;

        /// <summary>
        /// Gets the amount of items in this player's inventory.
        /// </summary>
        public int ItemCount => Inventory.UserInventory.Items.Count;

        /// <summary>
        /// Whether or not the player has any items.
        /// </summary>
        public bool HasAnyItems => Inventory.UserInventory.Items.Count > 0;

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
        public IReadOnlyCollection<ItemPickupBase> DroppedItems => _droppedItems;

        /// <summary>
        /// Gets permissions of the currently held keycard. <i>(<see cref="DoorPermissionFlags.None"/> if the player isn't holding a keycard)</i>.
        /// </summary>
        public DoorPermissionFlags HeldKeycardPermissions =>
            CurrentItem is KeycardItem keycardItem && keycardItem.TryGetDetail<PredefinedPermsDetail>(out var perms)
                    ? perms.Levels.Permissions
                    : DoorPermissionFlags.None;

        /// <summary>
        /// Whether or not to synchronize items with the player on the next frame.
        /// </summary>
        public bool IsSending
        {
            get => Inventory.SendItemsNextFrame;
            set => Inventory.SendItemsNextFrame = value;
        }

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

                instance.Owner = Inventory._hub;

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
        /// Gets or sets the player's <see cref="InventorySystem.Items.Usables.CurrentlyUsedItem"/>.
        /// </summary>
        public CurrentlyUsedItem CurrentlyUsedItem
        {
            get => UsableItemsHandler.CurrentUsable;
            set => UsableItemsHandler.CurrentUsable = value;
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
        public DoorPermissionFlags AllKeycardPermissions
        {
            get
            {
                var perms = DoorPermissionFlags.None;

                foreach (var keycard in Keycards)
                {
                    if (!keycard.TryGetDetail<PredefinedPermsDetail>(out var cardPerms))
                        continue;

                    perms |= cardPerms.Levels.Permissions;
                }

                return perms;
            }
        }

        /// <summary>
        /// Adds a new item to the player's inventory.
        /// </summary>
        /// <typeparam name="T">Generic type of the item.</typeparam>
        /// <param name="type">Type of the item.</param>
        /// <param name="addReason">Reason for this item being added.</param>
        /// <param name="itemSerial">The item's serial number.</param>
        /// <returns>The item instance as <typeparamref name="T"/>.</returns>
        public T AddItem<T>(ItemType type, ItemAddReason addReason = ItemAddReason.AdminCommand, ushort? itemSerial = null) where T : ItemBase
            => (T)AddItem(type, addReason, itemSerial);

        /// <summary>
        /// Adds a new item to the player's inventory.
        /// </summary>
        /// <param name="type">Type of the item.</param>
        /// <param name="addReason">Reason for this item being added.</param>
        /// <param name="itemSerial">The item's serial number.</param>
        /// <returns>The item instance.</returns>
        public ItemBase AddItem(ItemType type, ItemAddReason addReason = ItemAddReason.AdminCommand, ushort? itemSerial = null)
           => Inventory.ServerAddItem(type, addReason, itemSerial ?? 0);

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

        public bool HasKeycardPermission(DoorPermissionFlags keycardPermissions, bool anyPermission = false)
            => Keycards.Any(card =>
            {
                if (!card.TryGetDetail<PredefinedPermsDetail>(out var cardPerms))
                    return false;

                return anyPermission
                    ? cardPerms.Levels.Permissions.HasFlagAny(keycardPermissions)
                    : cardPerms.Levels.Permissions.HasFlagAll(keycardPermissions);
            });

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

        public bool AddOrSpawnItem(ItemType type, ItemAddReason addReason)
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

            return Inventory.ServerAddItem(type, addReason);
        }

        public T ThrowItem<T>(ItemBase item, float force = 1f, Vector3? scale = null) where T : ItemPickupBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            Inventory.ServerRemoveItem(item.ItemSerial, item.PickupDropModel);
            return ThrowItem<T>(item.ItemTypeId, force, scale, item.ItemSerial);
        }

        public T ThrowItem<T>(ItemType itemType, float force = 1f, Vector3? scale = null, ushort? itemSerial = null) where T : ItemPickupBase
        {
            var itemPrefab = itemType.GetItemPrefab<ItemBase>();
            
            var pickupInstance = itemType.GetPickupInstance<T>(Inventory._hub.PlayerCameraReference.position, scale, 
                Inventory._hub.PlayerCameraReference.rotation, itemSerial, true);
            var pickupRigidbody = pickupInstance?.GetRigidbody();

            if (pickupRigidbody is null)
                throw new Exception($"Pickup {itemType} cannot be thrown");

            LabApi.Events.Handlers.PlayerEvents.OnThrowingItem(new LabApi.Events.Arguments.PlayerEvents.PlayerThrowingItemEventArgs(Inventory._hub, pickupInstance, pickupRigidbody));
            
            var velocity = Inventory._hub.GetVelocity();
            var angular = Vector3.Lerp(itemPrefab.ThrowSettings.RandomTorqueA, itemPrefab.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

            velocity = velocity / 3f + Inventory._hub.PlayerCameraReference.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, pickupRigidbody.mass)) + 0.3f);

            velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
            velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
            velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));
            
            velocity *= force;

            pickupRigidbody.position = Inventory._hub.PlayerCameraReference.position;
            pickupRigidbody.velocity = velocity;
            pickupRigidbody.angularVelocity = angular;

            if (pickupRigidbody.angularVelocity.magnitude > pickupRigidbody.maxAngularVelocity)
                pickupRigidbody.maxAngularVelocity = pickupRigidbody.angularVelocity.magnitude;

            return pickupInstance;
        }

        public float GetPersonalUsableCooldown(ItemType usableItemType)
            => UsableItemsHandler.PersonalCooldowns.TryGetValue(usableItemType, out var cooldown) ? cooldown : 0f;

        public float GetGlobalUsableCooldown(ushort usableItemSerial)
            => UsableItemsController.GlobalItemCooldowns.TryGetValue(usableItemSerial, out var cooldown) ? cooldown : 0f;

        public void SetPersonalUsableCooldown(ItemType usableItemType, float cooldown)
            => UsableItemsHandler.PersonalCooldowns[usableItemType] = Time.timeSinceLevelLoad + cooldown;

        public void SetGlobalUsableCooldown(ushort usableItemSerial, float cooldown)
            => UsableItemsController.GlobalItemCooldowns[usableItemSerial] = cooldown;

        public void Synchronize()
            => Inventory.ServerSendItems();

        public void Dispose()
        {
            if (_droppedItems != null)
            {
                HashSetPool<ItemPickupBase>.Shared.Return(_droppedItems);
                _droppedItems = null;
            }
        }

        public override string ToString()
            => (CurrentItem?.ItemTypeId ?? ItemType.None).ToString();

        #region Operators
        public static implicit operator ItemBase(InventoryContainer container)
            => container?.CurrentItem;

        public static implicit operator ItemType(InventoryContainer container)
            => container?.CurrentItemType ?? ItemType.None;

        public static implicit operator ItemIdentifier(InventoryContainer container)
            => container?.CurrentItemIdentifier ?? ItemIdentifier.None;

        public static implicit operator ushort(InventoryContainer container)
            => container?.CurrentItem?.ItemSerial ?? 0;

        public static implicit operator bool(InventoryContainer container)
            => container != null && container.CurrentItem != null && container.CurrentItem;

        public static implicit operator string(InventoryContainer container)
            => (container?.CurrentItem?.ItemTypeId ?? ItemType.None).ToString();
        #endregion
    }
}