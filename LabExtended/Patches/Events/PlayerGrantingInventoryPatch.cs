using HarmonyLib;

using InventorySystem;
using InventorySystem.Configs;

using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.CustomRoles;
using LabExtended.Attributes;

using LabExtended.Core;
using LabExtended.Core.Hooking;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events.Player;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using PlayerRoles;

namespace LabExtended.Patches.Events
{
    public static class PlayerGrantingInventoryPatch
    {
        [HookPatch(typeof(PlayerGrantedInventoryArgs))]
        [HookPatch(typeof(PlayerGrantingInventoryArgs))]
        [HarmonyPatch(typeof(InventoryItemProvider), nameof(InventoryItemProvider.RoleChanged))]
        public static bool Prefix(ReferenceHub ply, PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            try
            {
                if (!newRole.ServerSpawnFlags.HasFlag(RoleSpawnFlags.AssignInventory))
                    return false;

                if (!ExPlayer.TryGet(ply, out var player))
                    return true;

                var hasEscaped = newRole.ServerSpawnReason is RoleChangeReason.Escaped;
                var dropItems = InventoryItemProvider.KeepItemsAfterEscaping;

                var itemsToAdd = ListPool<ItemType>.Shared.Rent();
                var ammoToAdd = DictionaryPool<ItemType, ushort>.Shared.Rent();

                var previousItems = new List<ItemPickupBase>();

                if (StartingInventories.DefinedInventories.TryGetValue(newRole.RoleTypeId, out var startingInv))
                {
                    itemsToAdd.AddRange(startingInv.Items);
                    ammoToAdd.AddRange(startingInv.Ammo);
                }

                var grantingArgs = new PlayerGrantingInventoryArgs(player, true, !dropItems || !hasEscaped, hasEscaped, dropItems, itemsToAdd, ammoToAdd);

                if (!HookRunner.RunEvent(grantingArgs, true))
                {
                    ListPool<ItemType>.Shared.Return(itemsToAdd);
                    DictionaryPool<ItemType, ushort>.Shared.Return(ammoToAdd);
                    
                    return false;
                }

                var customRoles = CustomRole.GetRoles(ply);

                foreach (var customRole in customRoles)
                {
                    if (!customRole.IsEnabled)
                        continue;
                    
                    customRole.OnGrantingInventory(grantingArgs);
                }

                if (grantingArgs.DropPreviousItems)
                {
                    if (ply.inventory.TryGetBodyArmor(out var bodyArmor))
                        bodyArmor.DontRemoveExcessOnDrop = true;

                    while (ply.inventory.UserInventory.Items.Count > 0)
                        previousItems.Add(ply.inventory.ServerDropItem(ply.inventory.UserInventory.Items.ElementAt(0).Key));

                    InventoryItemProvider.PreviousInventoryPickups[ply] = previousItems;
                }

                if (grantingArgs.ShouldResetInventory)
                {
                    while (ply.inventory.UserInventory.Items.Count > 0)
                        ply.inventory.ServerRemoveItem(ply.inventory.UserInventory.Items.ElementAt(0).Key, null);

                    ply.inventory.UserInventory.ReserveAmmo.Clear();
                    ply.inventory.ServerSendAmmo();
                }

                var addedItems = ListPool<ItemBase>.Shared.Rent();

                if (grantingArgs.ShouldGrantInventory)
                {
                    foreach (var itemType in itemsToAdd)
                    {
                        var item = ply.inventory.ServerAddItem(itemType, ItemAddReason.StartingItem);

                        if (item is null)
                            continue;

                        addedItems.Add(item);
                    }

                    foreach (var ammoType in ammoToAdd)
                        ply.inventory.ServerAddAmmo(ammoType.Key, ammoType.Value);
                }

                HookRunner.RunEvent(new PlayerGrantedInventoryArgs(player, hasEscaped, addedItems, previousItems, ammoToAdd));

                if (previousItems.Count < 1)
                {
                    InventoryItemProvider.InventoriesToReplenish.Remove(ply);
                    InventoryItemProvider.PreviousInventoryPickups.Remove(ply);
                }

                ListPool<ItemType>.Shared.Return(itemsToAdd);
                ListPool<ItemBase>.Shared.Return(addedItems);

                DictionaryPool<ItemType, ushort>.Shared.Return(ammoToAdd);
                return false;
            }
            catch (Exception ex)
            {
                ApiLog.Error("PlayerGrantingInventoryPatch", ex);
                return true;
            }
        }
    }
}