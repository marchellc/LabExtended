using HarmonyLib;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Items.Custom.Item;
using LabExtended.API.Items.Custom.Pickup;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Extensions;

using LabExtended.Utilities;
using LabExtended.Utilities.Unity;

namespace LabExtended.API.Items.Custom
{
    public static class CustomItemManager
    {
        public struct CustomItemsUpdateSegment { }

        private static LockedDictionary<Type, CustomItem> _registeredItems = new LockedDictionary<Type, CustomItem>();

        internal static LockedDictionary<ushort, CustomItemInventoryBehaviour> _inventoryItems = new LockedDictionary<ushort, CustomItemInventoryBehaviour>();
        internal static LockedDictionary<ushort, CustomItemPickupBehaviour> _pickupItems = new LockedDictionary<ushort, CustomItemPickupBehaviour>();

        public static IEnumerable<CustomItem> RegisteredItems => _registeredItems.Values;

        public static IEnumerable<CustomItemInventoryBehaviour> ItemsInInventory => _inventoryItems.Values;
        public static IEnumerable<CustomItemPickupBehaviour> ItemsDropped => _pickupItems.Values;

        public static List<T> GetInventoryItems<T>(ExPlayer owner) where T : CustomItemInventoryBehaviour 
            => TryGetInventoryItems<T>(owner, out var items) ? items : items;

        public static List<T> GetPickupItems<T>(ExPlayer owner) where T : CustomItemPickupBehaviour
            => TryGetPickupItems<T>(owner, out var items) ? items : items;
            
        public static bool TryGetInventoryItems<T>(ExPlayer owner, out List<T> inventoryBehaviours) where T : CustomItemInventoryBehaviour
        {
            if (!owner)
                throw new ArgumentNullException(nameof(owner));

            inventoryBehaviours = new List<T>();

            foreach (var behaviour in _inventoryItems)
            {
                var player = behaviour.Value.Owner;

                if (player is null || !player)
                    continue;

                if (player != owner)
                    continue;

                if (behaviour.Value is not T tBehaviour)
                    continue;

                inventoryBehaviours.Add(tBehaviour);
            }

            return inventoryBehaviours.Count > 0;
        }

        public static bool TryGetPickupItems<T>(ExPlayer owner, out List<T> pickupBehaviours) where T : CustomItemPickupBehaviour
        {
            if (!owner)
                throw new ArgumentNullException(nameof(owner));

            pickupBehaviours = new List<T>();

            foreach (var behaviour in _pickupItems)
            {
                var player = behaviour.Value.Owner;

                if (player is null || !player)
                    continue;

                if (player != owner)
                    continue;

                if (behaviour.Value is not T tBehaviour)
                    continue;

                pickupBehaviours.Add(tBehaviour);
            }

            return pickupBehaviours.Count > 0;
        }

        public static bool TryGetPickupBehaviour(ushort pickupId, out CustomItemPickupBehaviour customItemPickupBehaviour)
            => _pickupItems.TryGetValue(pickupId, out customItemPickupBehaviour);

        public static bool TryGetInventoryBehaviour(ushort itemId, out CustomItemInventoryBehaviour customItemInventoryBehaviour)
            => _inventoryItems.TryGetValue(itemId, out customItemInventoryBehaviour);

        public static bool TryGetPickupBehaviour<T>(ushort pickupId, out T behaviour) where T : CustomItemPickupBehaviour
        {
            if (!_pickupItems.TryGetValue(pickupId, out var customItemPickupBehaviour) || customItemPickupBehaviour is not T tBehaviour)
            {
                behaviour = default;
                return false;
            }

            behaviour = tBehaviour;
            return true;
        }

        public static bool TryGetInventoryBehaviour<T>(ushort itemId, out T behaviour) where T : CustomItemInventoryBehaviour
        {
            if (!_inventoryItems.TryGetValue(itemId, out var customItemInventoryBehaviour) || customItemInventoryBehaviour is not T tBehaviour)
            {
                behaviour = default;
                return false;
            }

            behaviour = tBehaviour;
            return true;
        }

        public static bool TryGet<T>(string nameOrId, out T customItem) where T : CustomItem
        {
            if (!TryGet(nameOrId, out var item))
            {
                customItem = default;
                return false;
            }

            customItem = (T)item;
            return true;
        }

        public static bool TryGet(string nameOrId, out CustomItem customItem)
        {
            if (string.IsNullOrWhiteSpace(nameOrId))
            {
                customItem = null;
                return false;
            }

            foreach (var pair in _registeredItems)
            {
                if (pair.Value.Name == nameOrId || pair.Value.Id == nameOrId)
                {
                    customItem = pair.Value;
                    return true;
                }
            }

            customItem = null;
            return false;
        }

        public static bool TryGet<T>(out T customItem) where T : CustomItem
        {
            if (!_registeredItems.TryGetValue(typeof(T), out var item))
            {
                customItem = default;
                return false;
            }

            if (item is not T tItem)
                throw new Exception($"CustomItem {item.GetType().FullName} is not stored as it's type ({typeof(T).FullName}) .. how did this happen");

            customItem = tItem;
            return true;
        }

        public static bool TryGet(Type type, out CustomItem customItem)
        {
            if (type is null)
            {
                customItem = null;
                return false;
            }

            return _registeredItems.TryGetValue(type, out customItem);
        }

        public static bool RegisterItem(CustomItem customItem)
        {
            if (customItem is null)
                return false;

            if (string.IsNullOrWhiteSpace(customItem.Name) || string.IsNullOrWhiteSpace(customItem.Id))
                return false;

            if (customItem.InventoryType is null && customItem.PickupType is null)
                return false;

            if (customItem.InventoryBehaviourType is null && customItem.PickupBehaviourType is null)
                return false;

            if (customItem.InventoryBehaviourType != null && !customItem.InventoryBehaviourType.InheritsType<CustomItemInventoryBehaviour>())
                return false;

            if (customItem.PickupBehaviourType != null && !customItem.PickupBehaviourType.InheritsType<CustomItemPickupBehaviour>())
                return false;

            if (_registeredItems.Any(x => x.Value.Name == customItem.Name || x.Value.Id == customItem.Id))
                return false;

            var type = customItem.GetType();

            if (_registeredItems.ContainsKey(type))
                return false;

            customItem.inventoryConstructor ??= GetConstructor(customItem.InventoryBehaviourType);
            customItem.pickupConstructor ??= GetConstructor(customItem.PickupBehaviourType);

            _registeredItems.Add(type, customItem);
            return true;
        }

        private static Func<object[], object> GetConstructor(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            var emptyConstructor = AccessTools.Constructor(type, Type.EmptyTypes);

            if (emptyConstructor is null)
                throw new Exception($"Type '{type.FullName}' does not have an empty constructor.");

            return FastReflection.ForConstructor(emptyConstructor);
        }

        [LoaderInitialize(1)]
        internal static void OnLoad()
        {
            PlayerLoopHelper.System.InjectBefore(OnUpdate, typeof(PlayerLoopHelper.CustomBeforePlayerLoop), typeof(CustomItemsUpdateSegment));
        }

        private static void OnUpdate()
        {
            try
            {
                if (_registeredItems.Count < 1)
                    return;

                foreach (var customItem in _registeredItems)
                {
                    try
                    {
                        customItem.Value.OnUpdate();
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("Custom Items", $"An error occurred while executing update method of CustomItem &1{customItem.Value.GetType().FullName}&r:\n{ex.ToColoredString()}");
                    }
                }

                foreach (var inventoryBehaviour in _inventoryItems)
                {
                    try
                    {
                        if (!inventoryBehaviour.Value.IsEnabled)
                            continue;

                        inventoryBehaviour.Value.InternalOnUpdate();
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("Custom Items", $"An error occurred while executing update method of InventoryBehaviour &1{inventoryBehaviour.Value.GetType().FullName}&r:\n{ex.ToColoredString()}");
                    }
                }

                foreach (var pickupBehaviour in _pickupItems)
                {
                    try
                    {
                        if (!pickupBehaviour.Value.IsEnabled)
                            continue;

                        pickupBehaviour.Value.InternalOnUpdate();
                    }
                    catch (Exception ex)
                    {
                        ApiLog.Error("Custom Items", $"An error occurred while executing update method of PickupBehaviour &1{pickupBehaviour.Value.GetType().FullName}&r:\n{ex.ToColoredString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                ApiLog.Error("Custom Items", ex);
            }
        }
    }
}