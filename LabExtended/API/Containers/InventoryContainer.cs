using Interactables.Interobjects.DoorUtils;

using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Firearms;
using InventorySystem.Items.Keycards;
using InventorySystem.Items.Pickups;

using LabExtended.Extensions;

using UnityEngine;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;

using InventorySystem.Items.Usables;
using LabExtended.API.Custom.Items;
using LabExtended.Core.Pooling.Pools;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace LabExtended.API.Containers;

/// <summary>
/// A class used to manage player inventories.
/// </summary>
public class InventoryContainer : IDisposable
{
    internal HashSet<ItemPickupBase>? droppedItems;
    internal Dictionary<ushort, CustomItem> ownedCustomItems;

    /// <summary>
    /// Creates a new <see cref="InventoryContainer"/> instance.
    /// </summary>
    /// <param name="inventory">The player <see cref="InventorySystem.Inventory"/> component.</param>
    /// <param name="player">The player targeted by this container.</param>
    public InventoryContainer(Inventory inventory, ExPlayer player)
    {
        Inventory = inventory;
        Player = player;
        
        Snake = new(player);

        if (CurrentItem is ChaosKeycardItem chaosKeycard)
            Snake.Keycard = chaosKeycard;

        UsableItemsHandler = UsableItemsController.GetHandler(inventory._hub);

        droppedItems = HashSetPool<ItemPickupBase>.Shared.Rent();
        ownedCustomItems = DictionaryPool<ushort, CustomItem>.Shared.Rent();
    }

    /// <summary>
    /// Gets or sets an item in the player's inventory.
    /// </summary>
    /// <param name="itemSerial">The serial of the item.</param>
    public ItemBase? this[ushort itemSerial]
    {
        get => UserInventory.Items.TryGetValue(itemSerial, out var item) ? item : null;
        set
        {
            if (value != null)
            {
                value.TransferItem(Player.ReferenceHub);
            }
            else if (UserInventory.Items.TryGetValue(itemSerial, out var item))
            {
                item.DestroyItem();
            }
        }
    }

    /// <summary>
    /// Gets the targeted player's <see cref="InventorySystem.Inventory"/> component.
    /// </summary>
    public Inventory Inventory { get; }

    /// <summary>
    /// Gets the player targeted by this container.
    /// </summary>
    public ExPlayer Player { get; }

    /// <summary>
    /// Gets the targeted player's <see cref="PlayerHandler"/> usable item handler.
    /// </summary>
    public PlayerHandler UsableItemsHandler { get; }

    /// <summary>
    /// Gets the inventory item holder.
    /// </summary>
    public InventoryInfo UserInventory => Inventory.UserInventory;
    
    /// <summary>
    /// Gets the Snake minigame wrapper.
    /// </summary>
    public SnakeInfo Snake { get; private set; }

    /// <summary>
    /// Gets the amount of items in this player's inventory.
    /// </summary>
    public int ItemCount => Inventory.UserInventory.Items.Count;

    /// <summary>
    /// Gets the amount of custom items owned by this player.
    /// </summary>
    public int CustomItemCount => ownedCustomItems.Count;

    /// <summary>
    /// Whether or not the player has any items.
    /// </summary>
    public bool HasAnyItems => Inventory.UserInventory.Items.Count > 0;

    /// <summary>
    /// Gets a value indicating whether any custom items are currently owned.
    /// </summary>
    public bool HasAnyCustomItems => ownedCustomItems.Count > 0;

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
    public IReadOnlyCollection<ItemPickupBase> DroppedItems => droppedItems;

    /// <summary>
    /// Gets a list of all custom items owned by this player.
    /// </summary>
    public IReadOnlyDictionary<ushort, CustomItem> CustomItems => ownedCustomItems;

    /// <summary>
    /// Gets permissions of the currently held keycard. <i>(<see cref="DoorPermissionFlags.None"/> if the player isn't holding a keycard)</i>.
    /// </summary>
    public DoorPermissionFlags HeldKeycardPermissions =>
        CurrentItem is KeycardItem keycardItem && keycardItem.Details.TryGetFirst<PredefinedPermsDetail>(out var perms)
            ? perms.Levels.Permissions
            : DoorPermissionFlags.None;

    /// <summary>
    /// Gets the currently held custom item.
    /// </summary>
    public CustomItem? CurrentCustomItem { get; internal set; }

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
    /// Gets or sets the type of the currently selected item in the inventory.
    /// </summary>
    /// <remarks>Setting this property to a value other than <see cref="ItemType.None"/> will select the first
    /// matching item in the inventory, or create a new instance if none exists. Setting it to <see
    /// cref="ItemType.None"/> will clear the current selection.</remarks>
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

            if (Items.TryGetFirst(x => x.ItemTypeId == value, out var invItem))
            {
                CurrentItemIdentifier = invItem.ItemId;
                return;
            }

            var instance = value.GetItemInstance<ItemBase>();

            if (instance == null)
                throw new Exception($"Could not make an item instance of type {value}");

            instance.Owner = Inventory._hub;
            instance.OnAdded(null);

            CurrentItemIdentifier = instance.ItemId;
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
        set => Clear(value);
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
                if (!keycard.Details.TryGetFirst<PredefinedPermsDetail>(out var cardPerms))
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
    public T AddItem<T>(ItemType type, ItemAddReason addReason = ItemAddReason.AdminCommand, ushort? itemSerial = null)
        where T : ItemBase
        => (T)AddItem(type, addReason, itemSerial);

    /// <summary>
    /// Adds a new item to the player's inventory.
    /// </summary>
    /// <param name="type">Type of the item.</param>
    /// <param name="addReason">Reason for this item being added.</param>
    /// <param name="itemSerial">The item's serial number.</param>
    /// <returns>The item instance.</returns>
    public ItemBase AddItem(ItemType type, ItemAddReason addReason = ItemAddReason.AdminCommand,
        ushort? itemSerial = null)
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

    /// <summary>
    /// Drops a list of items cast to a specific pickup type. 
    /// </summary>
    /// <param name="types">The types to drop.</param>
    /// <typeparam name="T">The pickup type to cast to.</typeparam>
    /// <returns>A list of dropped pickups.</returns>
    public IEnumerable<T> DropItems<T>(params ItemType[] types) where T : ItemPickupBase
        => DropItems(item => item.PickupDropModel != null && item.PickupDropModel is T)
            .Where<T>(item => types.Length < 1 || types.Contains(item.Info.ItemId));

    /// <summary>
    /// Gets a list of items matching the type filter.
    /// </summary>
    /// <param name="types">The type filter.</param>
    /// <returns>A list of items.</returns>
    public IEnumerable<ItemBase> GetItems(params ItemType[] types)
        => Items.Where(item => types.Contains(item.ItemTypeId));

    /// <summary>
    /// Gets a list of items cast to a specific type.
    /// </summary>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <returns>A list of items cast to a specific type.</returns>
    public IEnumerable<T> GetItems<T>() where T : ItemBase
        => Items.Where<T>();

    /// <summary>
    /// Gets a list of items cast to a specific type with a type filter.
    /// </summary>
    /// <param name="type">The type filter.</param>
    /// <typeparam name="T">The type to cast to.</typeparam>
    /// <returns>A list of cast matching items.</returns>
    public IEnumerable<T> GetItems<T>(ItemType type) where T : ItemBase
        => Items.Where<T>(item => item.ItemTypeId == type);

    /// <summary>
    /// Whether or not the player has an item of a specific type.
    /// </summary>
    /// <param name="type">The type filter.</param>
    /// <returns>true if the player has an item of type specified in the type filter</returns>
    public bool HasItem(ItemType type)
        => Items.Any(it => it.ItemTypeId == type);

    /// <summary>
    /// Whether or not the player has an item of a specific type.
    /// </summary>
    /// <param name="type">The type filter.</param>
    /// <param name="item">The found item.</param>
    /// <returns>true if the player has an item of type specified in the type filter</returns>
    public bool HasItem(ItemType type, out ItemBase? item)
        => Items.TryGetFirst(x => x != null && x.ItemTypeId == type, out item);

    /// <summary>
    /// Whether or not the player has an item of a specific type.
    /// </summary>
    /// <param name="type">The type filter.</param>
    /// <param name="item">The found item.</param>
    /// <typeparam name="T">The cast item type.</typeparam>
    /// <returns>true if the player has an item of type specified in the type filter</returns>
    public bool HasItem<T>(ItemType type, out T? item) where T : ItemBase
        => Items.TryGetFirst(x => x != null && x.ItemTypeId == type, out item);

    /// <summary>
    /// Whether or not the player has at least the specified amount of items.
    /// </summary>
    /// <param name="type">The type filter.</param>
    /// <param name="count">The required amount.</param>
    /// <returns>true if the player has at least the specified amount of items</returns>
    public bool HasItems(ItemType type, int count)
        => Items.Count(it => it.ItemTypeId == type) >= count;

    /// <summary>
    /// Whether or not the player has a specific keycard permission.
    /// </summary>
    /// <param name="keycardPermissions">The required permission flags.</param>
    /// <param name="anyPermission">Whether or not the player needs to have all of the required permissions.</param>
    /// <returns>true if the player has the required permissions</returns>
    public bool HasKeycardPermission(DoorPermissionFlags keycardPermissions, bool anyPermission = false)
        => Keycards.Any(card =>
        {
            if (!card.Details.TryGetFirst<PredefinedPermsDetail>(out var cardPerms))
                return false;

            return anyPermission
                ? cardPerms.Levels.Permissions.HasFlagAny(keycardPermissions)
                : cardPerms.Levels.Permissions.HasFlagAll(keycardPermissions);
        });

    /// <summary>
    /// Counts the amount of items of a specific type.
    /// </summary>
    /// <param name="type">The item type to count.</param>
    /// <returns>The amount of items.</returns>
    public int CountItems(ItemType type)
        => Items.Count(it => it.ItemTypeId == type);

    /// <summary>
    /// Gets the item at a specific inventory slot.
    /// </summary>
    /// <param name="slotNumber">The target inventory slot.</param>
    /// <returns>The item at the specified slot (or null if none).</returns>
    public ItemBase? GetItemAtSlot(byte slotNumber)
    {
        if (slotNumber > 8)
            return null;

        return UserInventory.Items.ElementAtOrDefault(slotNumber).Value;
    }

    /// <summary>
    /// Selects the specified item.
    /// </summary>
    /// <param name="item">The item to select.</param>
    /// <returns>true if the item was selected</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Select(ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (!UserInventory.Items.ContainsKey(item.ItemSerial))
            return false;

        if (CurrentItemIdentifier.SerialNumber == item.ItemSerial)
            return false;
        
        Inventory.ServerSelectItem(item.ItemSerial);
        return true;
    }
    
    /// <summary>
    /// Selects an item with a specific serial number.
    /// </summary>
    /// <param name="itemSerial">The serial number of the item to select.</param>
    /// <returns>true if the item was selected</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Select(ushort itemSerial)
    {
        if (!UserInventory.Items.TryGetValue(itemSerial, out var item))
            return false;

        if (CurrentItemIdentifier.SerialNumber == item.ItemSerial)
            return false;
        
        Inventory.ServerSelectItem(item.ItemSerial);
        return true;
    }

    /// <summary>
    /// Selects an item at a specific inventory slot.
    /// </summary>
    /// <param name="slotNumber">The number of the inventory slot to select.</param>
    /// <returns>true if the item was selected</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool SelectSlot(byte slotNumber)
    {
        var item = GetItemAtSlot(slotNumber);

        if (item == null)
            return false;
        
        if (CurrentItemIdentifier.SerialNumber == item.ItemSerial)
            return false;
        
        Inventory.ServerSelectItem(item.ItemSerial);
        return true;
    }

    /// <summary>
    /// Removes an item of a specific serial number.
    /// </summary>
    /// <param name="serial">The serial number to remove.</param>
    /// <returns>true if the item was successfully removed</returns>
    public bool RemoveItem(ushort serial)
    {
        if (!UserInventory.Items.TryGetValue(serial, out var item))
            return false;

        item.DestroyItem();
        return true;
    }

    /// <summary>
    /// Removes an item of a specific serial number.
    /// </summary>
    /// <param name="serial">The serial number to remove.</param>
    /// <param name="item">The removed item instance.</param>
    /// <returns>true if the item was successfully removed</returns>
    public bool RemoveItem(ushort serial, out ItemBase? item)
    {
        if (!UserInventory.Items.TryGetValue(serial, out item))
            return false;

        item.DestroyItem();
        return true;
    }

    /// <summary>
    /// Removes a specific item instance.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool RemoveItem(ItemBase item)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        if (!UserInventory.Items.ContainsKey(item.ItemSerial))
            return false;

        item.DestroyItem();
        return true;
    }

    /// <summary>
    /// Removes the currently held item.
    /// </summary>
    /// <returns>true if the item was removed</returns>
    public bool RemoveHeldItem()
    {
        if (CurrentItem is null)
            return false;

        CurrentItem.DestroyItem();
        return true;
    }

    /// <summary>
    /// Removes the currently held item.
    /// </summary>
    /// <param name="item">The removed item instance.</param>
    /// <returns>true if the item was removed</returns>
    public bool RemoveHeldItem(out ItemBase? item)
    {
        if (CurrentItem is null)
        {
            item = null;
            return false;
        }

        item = CurrentItem;

        CurrentItem.DestroyItem();
        return true;
    }

    /// <summary>
    /// Removes items that match a condition.
    /// </summary>
    /// <param name="predicate">The condition.</param>
    /// <returns>The amount of removed items.</returns>
    public int RemoveItems(Predicate<ItemBase>? predicate = null)
    {
        var items = ListPool<ItemBase>.Shared.Rent(Items);
        var count = 0;

        foreach (var item in items)
        {
            if (predicate != null && !predicate(item))
                continue;

            item.DestroyItem();
        }

        ListPool<ItemBase>.Shared.Return(items);
        return count;
    }

    /// <summary>
    /// Removes an amount of items of type.
    /// </summary>
    /// <param name="type">The type of item to remove.</param>
    /// <param name="count">The amount of items to remove.</param>
    /// <param name="throwIfNotEnough">Whether or not to throw an exception if the player's current item count is not enough.</param>
    /// <returns>The amount of removed items.</returns>
    public int RemoveItems(ItemType type, int count = 1, bool throwIfNotEnough = false)
    {
        if (type is ItemType.None)
            return 0;

        if (type.IsAmmo())
        {
            Player.Ammo.SubstractAmmo(type, (ushort)count);
            return count;
        }

        if (throwIfNotEnough && CountItems(type) < count)
            throw new Exception($"There are not enough items ({type}) to remove! ({count})");

        var items = ListPool<ItemBase>.Shared.Rent(Items);
        var removed = 0;

        foreach (var item in items)
        {
            if (item.ItemTypeId != type)
                continue;

            if (removed >= count)
                break;

            removed++;

            item.DestroyItem();
        }

        ListPool<ItemBase>.Shared.Return(items);
        return removed;
    }

    /// <summary>
    /// Clears the player's inventory (and optionally assigns a new one).
    /// </summary>
    /// <param name="newInventory">The new inventory.</param>
    public void Clear(IEnumerable<ItemType>? newInventory = null)
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

            Inventory.SendItemsNextFrame = true;
        }
    }

    /// <summary>
    /// Adds an item to the player's inventory (or spawns it on the ground if full).
    /// </summary>
    /// <param name="type">The item to add.</param>
    /// <param name="addReason">The reason of adding this item.</param>
    /// <returns>true if the item was added to inventory, false if it was spawned on ground</returns>
    public bool AddOrSpawnItem(ItemType type, ItemAddReason addReason = ItemAddReason.AdminCommand)
    {
        if (Inventory.UserInventory.Items.Count > 7)
        {
            if (type.TryGetItemPrefab(out var itemPrefab))
                Inventory.ServerCreatePickup(itemPrefab, new PickupSyncInfo(type, itemPrefab.Weight));
            else
                throw new Exception($"Could not get prefab of item {type}");

            return false;
        }

        return Inventory.ServerAddItem(type, addReason) != null;
    }

    /// <summary>
    /// Throws the specified item.
    /// </summary>
    /// <param name="item">The item to throw.</param>
    /// <param name="force">Throwing force.</param>
    /// <param name="scale">The item's pickup scale.</param>
    /// <typeparam name="T">The pickup type to cast to.</typeparam>
    /// <returns>The thrown pickup instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public T ThrowItem<T>(ItemBase item, float force = 1f, Vector3? scale = null) where T : ItemPickupBase
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var serial = item.ItemSerial;
        var type = item.ItemTypeId;

        item.DestroyItem();

        return ThrowItem<T>(type, force, scale, serial);
    }

    /// <summary>
    /// Throws the specified item type.
    /// </summary>
    /// <param name="itemType">The type to throw.</param>
    /// <param name="force">Throwing force.</param>
    /// <param name="scale">Item pickup scale.</param>
    /// <param name="itemSerial">Item pickup serial.</param>
    /// <typeparam name="T">Item pickup cast type.</typeparam>
    /// <returns>The thrown pickup instance.</returns>
    /// <exception cref="Exception"></exception>
    public T ThrowItem<T>(ItemType itemType, float force = 1f, Vector3? scale = null, ushort? itemSerial = null)
        where T : ItemPickupBase
    {
        if (!itemType.TryGetItemPrefab(out var itemPrefab))
            throw new Exception($"Could not get prefab of item {itemType}");

        var pickupInstance = itemType.GetPickupInstance<T>(Inventory._hub.PlayerCameraReference.position, scale,
            Inventory._hub.PlayerCameraReference.rotation, itemSerial, true);
        var pickupRigidbody = pickupInstance?.GetRigidbody();

        if (pickupRigidbody == null)
            throw new Exception($"Pickup {itemType} cannot be thrown");

        var velocity = Inventory._hub.GetVelocity();
        var angular = Vector3.Lerp(itemPrefab.ThrowSettings.RandomTorqueA, itemPrefab.ThrowSettings.RandomTorqueB,
            UnityEngine.Random.value);

        velocity = velocity / 3f + Inventory._hub.PlayerCameraReference.forward * 6f *
            (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, pickupRigidbody.mass)) + 0.3f);

        velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (float)((!(velocity.x < 0f)) ? 1 : (-1));
        velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (float)((!(velocity.y < 0f)) ? 1 : (-1));
        velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (float)((!(velocity.z < 0f)) ? 1 : (-1));

        velocity *= force;

        pickupRigidbody.position = Inventory._hub.PlayerCameraReference.position;
        pickupRigidbody.linearVelocity = velocity;
        pickupRigidbody.angularVelocity = angular;

        if (pickupRigidbody.angularVelocity.magnitude > pickupRigidbody.maxAngularVelocity)
            pickupRigidbody.maxAngularVelocity = pickupRigidbody.angularVelocity.magnitude;

        return pickupInstance;
    }

    /// <summary>
    /// Gets the personal cooldown (in seconds) of a usable item.
    /// </summary>
    /// <param name="usableItemType">The item type.</param>
    /// <returns>The cooldown (in seconds).</returns>
    public float GetPersonalUsableCooldown(ItemType usableItemType)
        => UsableItemsHandler.PersonalCooldowns.TryGetValue(usableItemType, out var cooldown) ? cooldown : 0f;

    /// <summary>
    /// Gets the global cooldown (in seconds) of a usable item.
    /// </summary>
    /// <param name="usableItemSerial">The item serial.</param>
    /// <returns>The cooldown (in seconds).</returns>
    public float GetGlobalUsableCooldown(ushort usableItemSerial)
        => UsableItemsController.GlobalItemCooldowns.TryGetValue(usableItemSerial, out var cooldown) ? cooldown : 0f;

    /// <summary>
    /// Sets the personal cooldown (in seconds) of a usable item type.
    /// </summary>
    /// <param name="usableItemType">The item type.</param>
    /// <param name="cooldown">The cooldown time (in seconds).</param>
    public void SetPersonalUsableCooldown(ItemType usableItemType, float cooldown)
        => UsableItemsHandler.PersonalCooldowns[usableItemType] = Time.timeSinceLevelLoad + cooldown;

    /// <summary>
    /// Sets the global cooldown (in seconds) of a usable item serial.
    /// </summary>
    /// <param name="usableItemSerial">The item serial.</param>
    /// <param name="cooldown">The cooldown time (in seconds).</param>
    public void SetGlobalUsableCooldown(ushort usableItemSerial, float cooldown)
        => UsableItemsController.GlobalItemCooldowns[usableItemSerial] = cooldown;

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        if (droppedItems != null)
            HashSetPool<ItemPickupBase>.Shared.Return(droppedItems);

        if (ownedCustomItems != null)
            DictionaryPool<ushort, CustomItem>.Shared.Return(ownedCustomItems);
        
        Snake?.Reset(false, true);
        Snake = null;

        CurrentCustomItem = null;
        
        droppedItems = null;
        ownedCustomItems = null;
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => (CurrentItem?.ItemTypeId ?? ItemType.None).ToString();

    #region Operators

    /// <summary>
    /// Converts the container to an item (currently held item).
    /// </summary>
    /// <param name="container">The container to convert.</param>
    /// <returns>The converted item.</returns>
    public static implicit operator ItemBase?(InventoryContainer container)
        => container?.CurrentItem;

    /// <summary>
    /// Converts the container to an item type (currently held item type).
    /// </summary>
    /// <param name="container">The container to convert.</param>
    /// <returns>The converted item type.</returns>
    public static implicit operator ItemType(InventoryContainer container)
        => container?.CurrentItemType ?? ItemType.None;

    /// <summary>
    /// Converts the container to an item (currently held item identifier).
    /// </summary>
    /// <param name="container">The container to convert.</param>
    /// <returns>The converted item identifier.</returns>
    public static implicit operator ItemIdentifier(InventoryContainer container)
        => container?.CurrentItemIdentifier ?? ItemIdentifier.None;

    /// <summary>
    /// Converts the container to an item serial (currently held item serial).
    /// </summary>
    /// <param name="container">The container to convert.</param>
    /// <returns>The converted item serial.</returns>
    public static implicit operator ushort(InventoryContainer container)
        => container?.CurrentItem?.ItemSerial ?? 0;

    /// <summary>
    /// Converts the container to an item name (currently held item type).
    /// </summary>
    /// <param name="container">The container to convert.</param>
    /// <returns>The converted item name.</returns>
    public static implicit operator string(InventoryContainer container)
        => (container?.CurrentItem?.ItemTypeId ?? ItemType.None).ToString();

    #endregion
}