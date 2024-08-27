using InventorySystem;
using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabExtended.API.Collections.Locked;
using LabExtended.API.CustomItems.Info;
using LabExtended.API.CustomItems.Interfaces;

using LabExtended.Core;
using LabExtended.Core.Ticking;
using LabExtended.Events.Player;

using LabExtended.Extensions;

using PluginAPI.Events;

using UnityEngine;

namespace LabExtended.API.CustomItems
{
    public class CustomItem
    {
        static CustomItem()
            => TickDistribution.UnityTick.CreateHandle(TickDistribution.CreateWith(TickItems));

        private static readonly List<ICustomItemInfo> _registeredTypes = new List<ICustomItemInfo>();
        private static readonly LockedDictionary<ushort, CustomItem> _items = new LockedDictionary<ushort, CustomItem>();

        public static IReadOnlyList<ICustomItemInfo> RegisteredItems => _registeredTypes;
        public static IReadOnlyDictionary<ushort, CustomItem> ActiveItems => _items;

        public static int RegisteredCount => _registeredTypes.Count;
        public static int ActiveCount => _items.Count;

        public ushort Serial { get; internal set; }

        public ItemBase Item { get; internal set; }
        public ItemPickupBase Pickup { get; internal set; }

        public ExPlayer Owner { get; internal set; }

        public ICustomItemInfo Info { get; internal set; }

        public bool IsDropped => Pickup != null;
        public bool IsOwned => Owner != null;
        public bool IsInInventory => Item != null;

        public bool IsSelected { get; internal set; }

        public virtual void OnInventoryTicked() { }
        public virtual void OnSpawnedTicked() { }
        public virtual void OnTicked() { }

        public virtual void OnAdding() { }
        public virtual void OnAdded() { }

        public virtual void OnSpawning() { }
        public virtual void OnSpawned() { }

        public virtual void OnSelecting(PlayerSelectingItemArgs args) { }
        public virtual void OnSelected(PlayerSelectedItemArgs args) { }

        public virtual void OnDeselecting(PlayerSelectingItemArgs args) { }
        public virtual void OnDeselected(PlayerSelectedItemArgs args) { }

        public virtual void OnOwnerSpawning(PlayerSpawningArgs args) { }
        public virtual void OnOwnerSpawned(PlayerChangedRoleArgs args) { }

        public virtual bool OnDying(PlayerDyingEvent args) => true;

        public virtual void OnPickingUp(PlayerPickingUpItemArgs args) { }
        public virtual void OnPickedUp(PlayerPickingUpItemArgs args) { }

        public virtual void OnDropping(PlayerDroppingItemArgs args) { }
        public virtual void OnDropped(PlayerDroppingItemArgs args) { }

        public virtual void OnThrowing(PlayerThrowingItemArgs args) { }
        public virtual void OnThrown(PlayerThrowingItemArgs args) { }

        public void Select()
            => Owner?.Hub.inventory.UserCode_CmdSelectItem__UInt16(Serial);

        public void Give(ExPlayer player)
        {
            if (IsDropped)
            {
                var args = new PlayerPickingUpItemArgs(player, Pickup, null, null, null, true);

                OnPickingUp(args);

                if (!args.IsAllowed)
                    return;

                Owner?.Inventory._customItems.Remove(this);

                var item = Pickup.Info.ItemId.GetItemInstance<ItemBase>(Serial);

                item.SetupItem(player.Hub, false);

                Owner = player;
                Item = item;

                if (args.DestroyPickup)
                    Pickup.DestroySelf();

                Pickup = null;

                if (!player.Inventory._customItems.Contains(this))
                    player.Inventory._customItems.Add(this);

                OnPickedUp(args);
                SetupItem();
            }
            else if (IsInInventory)
            {
                var args = new PlayerPickingUpItemArgs(player, null, null, null, null, true);

                OnPickingUp(args);

                if (!args.IsAllowed)
                    return;

                Owner?.Inventory._customItems.Remove(this);

                Item.SetupItem(player.Hub, false);
                Owner = player;

                if (!player.Inventory._customItems.Contains(this))
                    player.Inventory._customItems.Add(this);

                OnPickedUp(args);
                SetupItem();
            }
            else
            {
                throw new Exception("This item isn't spawned or in inventory.");
            }
        }

        public void Drop()
        {
            if (IsDropped)
                throw new Exception("This item is already dropped.");

            var args = new PlayerDroppingItemArgs(Owner, Item, false);

            OnDropping(args);

            if (!args.IsAllowed)
                return;

            Item.OwnerInventory.ServerRemoveItem(Serial, Item.PickupDropModel);
            Item = null;

            var pickup = Info.PickupInfo.Type.GetPickupInstance<ItemPickupBase>(Owner.Position.Position, Info.PickupInfo.Scale, Owner.Rotation, Serial, true);

            Pickup = pickup;

            OnDropped(args);
            SetupPickup();
        }

        public void Throw()
        {
            if (IsDropped)
                throw new Exception("This item is already dropped.");

            var pickupInstance = Info.PickupInfo.Type.GetPickupInstance<ItemPickupBase>(Owner.Position, Info.PickupInfo.Scale, Owner.Rotation, Serial, true);
            var pickupRigidbody = pickupInstance.GetRigidbody();

            var velocity = Owner.Velocity;
            var angular = Vector3.Lerp(Item.ThrowSettings.RandomTorqueA, Item.ThrowSettings.RandomTorqueB, UnityEngine.Random.value);

            velocity = velocity / 3f + Owner.CameraTransform.forward * 6f * (Mathf.Clamp01(Mathf.InverseLerp(7f, 0.1f, pickupRigidbody.mass)) + 0.3f);

            velocity.x = Mathf.Max(Mathf.Abs(velocity.x), Mathf.Abs(velocity.x)) * (!(velocity.x < 0f) ? 1 : -1);
            velocity.y = Mathf.Max(Mathf.Abs(velocity.y), Mathf.Abs(velocity.y)) * (!(velocity.y < 0f) ? 1 : -1);
            velocity.z = Mathf.Max(Mathf.Abs(velocity.z), Mathf.Abs(velocity.z)) * (!(velocity.z < 0f) ? 1 : -1);

            var args = new PlayerThrowingItemArgs(Owner, Item, pickupInstance, pickupRigidbody, Owner.CameraTransform.position, velocity, angular);

            OnThrowing(args);

            if (!args.IsAllowed)
                return;

            pickupRigidbody.position = args.Position;
            pickupRigidbody.velocity = args.Velocity;
            pickupRigidbody.angularVelocity = args.AngularVelocity;

            if (pickupRigidbody.angularVelocity.magnitude > pickupRigidbody.maxAngularVelocity)
                pickupRigidbody.maxAngularVelocity = pickupRigidbody.angularVelocity.magnitude;

            Pickup = pickupInstance;

            Item.OwnerInventory.ServerRemoveItem(Serial, Item.PickupDropModel);
            Item = null;

            OnThrown(args);
            SetupPickup();
        }

        internal virtual void SetupItem() { }
        internal virtual void SetupPickup() { }

        public static bool RegisterItem<TItem>(string name, string id, string description, ItemType inventoryType, CustomItemPickupInfo pickupInfo, CustomItemFlags flags = CustomItemFlags.DropOnDeath) where TItem : CustomItem
            => RegisterItem(new CustomItemInfo(typeof(TItem), id, name, description, inventoryType, flags, pickupInfo));

        public static bool RegisterItem(ICustomItemInfo customItemInfo)
        {
            if (customItemInfo is null)
                throw new ArgumentNullException(nameof(customItemInfo));

            if (customItemInfo.Type is null)
                throw new ArgumentNullException(nameof(customItemInfo.Type));

            if (TryGetRegistered(customItemInfo.Type, out _))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register a duplicate custom item: &3{customItemInfo.Type.FullName}&r");
                return false;
            }

            if (string.IsNullOrEmpty(customItemInfo.Id))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register an item with an empty ID: &3{customItemInfo.Type.FullName}&r");
                return false;
            }

            if (string.IsNullOrWhiteSpace(customItemInfo.Name))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register an item with an empty name: &3{customItemInfo.Type.FullName}&r");
                return false;
            }

            if (string.IsNullOrWhiteSpace(customItemInfo.Description))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register an item with an empty description: &3{customItemInfo.Type.FullName}");
                return false;
            }

            if (TryGetRegistered(customItemInfo.Id, out _))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register a duplicate item ID: &3{customItemInfo.Id}&r (&6{customItemInfo.Type.FullName}&r)");
                return false;
            }

            if (TryGetRegistered(customItemInfo.Name, out _))
            {
                ApiLoader.Warn("Custom Items", $"Tried to register a duplicate item name: &3{customItemInfo.Name}&r (&6{customItemInfo.Type.FullName}&r)");
                return false;
            }

            _registeredTypes.Add(customItemInfo);
            return true;
        }

        public static bool UnregisterItem<TItem>() where TItem : CustomItem
            => _registeredTypes.RemoveAll(info => info.Type == typeof(TItem)) > 0;

        public static bool UnregisterItem(Type type)
            => _registeredTypes.RemoveAll(info => info.Type == type) > 0;

        public static bool TryGetRegistered<TItem>(out ICustomItemInfo customItemInfo) where TItem : CustomItem
            => _registeredTypes.TryGetFirst(info => info.Type == typeof(TItem), out customItemInfo);

        public static bool TryGetRegistered(Type type, out ICustomItemInfo customItemInfo)
            => _registeredTypes.TryGetFirst(info => info.Type == type, out customItemInfo);

        public static bool TryGetRegistered(string nameOrId, out CustomItemInfo customItemInfo)
            => _registeredTypes.TryGetFirst(info => info.Id == nameOrId || info.Name.GetSimilarity(nameOrId) >= 0.85, out customItemInfo);

        public static bool TryGetItem(ushort serial, out CustomItem customItem)
            => _items.TryGetValue(serial, out customItem);

        public static bool TryGetItem<T>(ushort serial, out T customItem) where T : CustomItem
            => (_items.TryGetValue(serial, out var instance) && instance is T ? customItem = (T)instance : customItem = null) != null;

        public static bool TryGetItem(ItemBase item, out CustomItem customItem)
            => _items.TryGetValue(item.ItemSerial, out customItem);

        public static bool TryGetItem<T>(ItemBase item, out T customItem) where T : CustomItem
            => (_items.TryGetValue(item.ItemSerial, out var instance) && instance is T ? customItem = (T)instance : customItem = null) != null;

        public static bool TryGetItem(ItemPickupBase pickup, out CustomItem customItem)
            => _items.TryGetValue(pickup.Info.Serial, out customItem);

        public static bool TryGetItem<T>(ItemPickupBase pickup, out T customItem) where T : CustomItem
            => (_items.TryGetValue(pickup.Info.Serial, out var item) && item is T ? customItem = (T)item : customItem = null) != null;

        public static TItem Give<TItem>(ExPlayer player, bool selectItem = false) where TItem : CustomItem
        {
            if (!TryGetRegistered<TItem>(out var itemInfo))
                throw new Exception($"Unregistered item type: {typeof(TItem).FullName}");

            if (itemInfo.InventoryType is ItemType.None)
                throw new Exception($"Item '{typeof(TItem).FullName}' cannot be added to inventory.");

            var customItem = itemInfo.Type.Construct<TItem>();
            var gameItem = player.Inventory.AddItem(itemInfo.InventoryType);

            player.Inventory._customItems.Add(customItem);

            customItem.Info = itemInfo;
            customItem.OnAdding();

            gameItem.SetupItem(player.Hub);

            customItem.Owner = player;

            customItem.Item = gameItem;
            customItem.Serial = gameItem.ItemSerial;

            customItem.SetupItem();
            customItem.OnAdded();

            _items[customItem.Serial] = customItem;

            if ((itemInfo.ItemFlags & CustomItemFlags.SelectOnPickup) == CustomItemFlags.SelectOnPickup || selectItem)
                customItem.Select();

            return customItem;
        }

        public static TItem Spawn<TItem>(Vector3 position, Quaternion? rotation = null, Vector3? scale = null, ExPlayer owner = null) where TItem : CustomItem
        {
            if (!TryGetRegistered<TItem>(out var itemInfo))
                throw new Exception($"Unregistered item type: {typeof(TItem).FullName}");

            if (itemInfo.PickupInfo.Type is ItemType.None)
                throw new Exception($"Item '{typeof(TItem).FullName}' cannot be spawned.");

            var itemScale = scale.HasValue ? scale.Value : itemInfo.PickupInfo.Scale;
            var itemRot = rotation.HasValue ? rotation.Value : Quaternion.identity;

            var customItem = itemInfo.Type.Construct<TItem>();
            var gamePickup = itemInfo.PickupInfo.Type.GetPickupInstance<ItemPickupBase>(position, itemScale, itemRot, null, true);

            owner?.Inventory._customItems.Add(customItem);

            customItem.Info = itemInfo;
            customItem.OnSpawning();

            customItem.Owner = owner;

            customItem.Pickup = gamePickup;
            customItem.Serial = gamePickup.Info.Serial;

            customItem.SetupPickup();
            customItem.OnSpawned();

            _items[customItem.Serial] = customItem;
            return customItem;
        }

        private static void TickItems()
        {
            foreach (var pair in _items)
            {
                try
                {
                    pair.Value.OnTicked();

                    if (pair.Value.IsDropped)
                        pair.Value.OnSpawnedTicked();
                    else if (pair.Value.IsInInventory)
                        pair.Value.OnInventoryTicked();
                }
                catch (Exception ex)
                {
                    ApiLoader.Error("Custom Items", $"Failed to tick custom item &3{pair.Value.Info.Id}&r (&6{pair.Value.Serial}&r):\n{ex.ToColoredString()}");
                }
            }
        }

        internal static bool InternalHandleDying(PlayerDyingEvent ev)
        {
            var isCancelled = false;
            var items = ev.Player.Items.ToList();

            foreach (var item in items)
            {
                if (item != null && TryGetItem(item, out var customItem))
                {
                    if (!customItem.OnDying(ev))
                    {
                        isCancelled = true;
                        continue;
                    }

                    ev.Player.RemoveItem(item);

                    if ((customItem.Info.ItemFlags & CustomItemFlags.DropOnDeath) == CustomItemFlags.DropOnDeath)
                        customItem.Drop();
                }
            }

            return !isCancelled;
        }

        internal static void InternalHandleWaiting()
            => _items.Clear();
    }
}