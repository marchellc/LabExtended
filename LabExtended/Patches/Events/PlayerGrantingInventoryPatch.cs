using HarmonyLib;

using InventorySystem;
using InventorySystem.Configs;

using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;
using InventorySystem.Items.Usables.Scp1344;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

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
        [HookPatch(typeof(PlayerGrantedInventoryArgs), true)]
        [HookPatch(typeof(PlayerGrantingInventoryArgs), true)]
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

                var receivingArgs = new PlayerReceivingLoadoutEventArgs(ply, itemsToAdd, ammoToAdd, !hasEscaped || !dropItems);
                
                PlayerEvents.OnReceivingLoadout(receivingArgs);

                if (!receivingArgs.IsAllowed)
                {
                    ListPool<ItemType>.Shared.Return(itemsToAdd);
                    DictionaryPool<ItemType, ushort>.Shared.Return(ammoToAdd);

                    return false;
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
                
                if (receivingArgs.InventoryReset || grantingArgs.ShouldResetInventory)
                {
                    while (ply.inventory.UserInventory.Items.Count > 0)
                        ply.inventory.ServerRemoveItem(ply.inventory.UserInventory.Items.ElementAt(0).Key, null);
                    
                    ply.inventory.UserInventory.ReserveAmmo.Clear();
                    ply.inventory.SendAmmoNextFrame = true;
                }

                if (grantingArgs.DropPreviousItems)
                {
                    if (ply.inventory.TryGetBodyArmor(out var bodyArmor))
                        bodyArmor.DontRemoveExcessOnDrop = true;

                    var itemCount = ply.inventory.UserInventory.Items.Count(x => x.Value is not Scp1344Item);
                    var removedCount = 0;

                    while (removedCount != itemCount)
                    {
                        var nextItem = ply.inventory.UserInventory.Items.ElementAt(0);

                        if (nextItem.Value is Scp1344Item scp1344)
                        {
                            scp1344.Status = Scp1344Status.Idle;
                            continue;
                        }

                        removedCount++;
                        
                        previousItems.Add(ply.inventory.ServerDropItem(nextItem.Key));
                    }

                    InventoryItemProvider.PreviousInventoryPickups[ply] = previousItems;
                }

                var addedItems = ListPool<ItemBase>.Shared.Rent();

                if (grantingArgs.ShouldGrantInventory)
                {
                    foreach (var ammoType in ammoToAdd)
                        ply.inventory.ServerAddAmmo(ammoType.Key, ammoType.Value);
                    
                    foreach (var itemType in itemsToAdd)
                    {
                        var item = ply.inventory.ServerAddItem(itemType, ItemAddReason.StartingItem);
                        if (item is null) continue;

                        addedItems.Add(item);
                        
                        InventoryItemProvider.OnItemProvided.InvokeSafe(ply, item);
                    }
                }

                HookRunner.RunEvent(new PlayerGrantedInventoryArgs(player, hasEscaped, addedItems, previousItems, ammoToAdd));
                PlayerEvents.OnReceivedLoadout(new PlayerReceivedLoadoutEventArgs(ply, itemsToAdd, ammoToAdd, receivingArgs.InventoryReset));

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