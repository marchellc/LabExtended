using HarmonyLib;

using InventorySystem.Items;
using InventorySystem.Items.Armor;
using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Scp914;
using LabExtended.API.Scp914.Outputs;
using LabExtended.API.Scp914.Interfaces;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using PlayerRoles.FirstPersonControl;

using Scp914;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp914;

/// <summary>
/// Implements custom item recipes.
/// </summary>
public static class UpgradeItemPatch
{
    public static FastEvent<Action<Scp914Result, Scp914KnobSetting>> OnUpgraded { get; } =
        FastEvents.DefineEvent<Action<Scp914Result, Scp914KnobSetting>>(typeof(Scp914Upgrader),
            nameof(Scp914Upgrader.OnUpgraded));
    
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.ProcessPlayer))]
    private static bool Prefix(ReferenceHub ply, bool upgradeInventory, bool heldOnly, Scp914KnobSetting setting)
    {
        if (!ApiLoader.ApiConfig.OtherSection.Scp914CustomRecipes)
            return true;
        
        if (!ExPlayer.TryGet(ply, out var player))
            return false;

        if (Physics.Linecast(ply.transform.position, Scp914Controller.Singleton.IntakeChamber.position,
                Scp914Upgrader.SolidObjectMask))
            return false;

        var position = ply.transform.position + Scp914Controller.MoveVector;
        var processingArgs = new Scp914ProcessingPlayerEventArgs(position, setting, ply);
        
        Scp914Events.OnProcessingPlayer(processingArgs);

        if (!processingArgs.IsAllowed)
            return false;

        setting = processingArgs.KnobSetting;
        position = processingArgs.NewPosition;

        ply.TryOverridePosition(position);

        if (!upgradeInventory)
            return false;

        var serials = HashSetPool<ushort>.Shared.Rent();
        var outputs = ListPool<IScp914Output>.Shared.Rent();
        var resultItems = ListPool<ItemBase>.Shared.Rent();
        var resultPickups = ListPool<ItemPickupBase>.Shared.Rent();

        foreach (var pair in ply.inventory.UserInventory.Items)
        {
            if (!heldOnly || pair.Key == ply.inventory.CurItem.SerialNumber)
            {
                serials.Add(pair.Key);
            }
        }

        foreach (var serial in serials)
        {
            if (!ply.inventory.UserInventory.Items.TryGetValue(serial, out var item))
                continue;
            
            var processingItemArgs = new Scp914ProcessingInventoryItemEventArgs(item, setting, ply);
            
            Scp914Events.OnProcessingInventoryItem(processingItemArgs);
            
            if (!processingItemArgs.IsAllowed)
                continue;

            setting = processingItemArgs.KnobSetting;
            
            Scp914Upgrader.OnInventoryItemUpgraded.InvokeSafe(item, setting);

            if (!Scp914Recipes.TryGetEntry(setting, item.ItemTypeId, out var entry) || entry is null)
            {
                ApiLog.Debug("SCP-914 API", $"Entry for &3{item.ItemTypeId}&r has not been found");
                continue;
            }

            var recipe = entry.Recipes.GetRandomWeighted(x => x.Chance);

            if (recipe is null)
            {
                ApiLog.Debug("SCP-914 API", "Selected a null recipe");
                continue;
            }

            outputs.Clear(); 
            
            recipe.Pick(player, outputs);

            var itemHeld = item.ItemSerial == ply.inventory.CurItem.SerialNumber;
            var itemSerial = item.ItemSerial;
            var itemUsed = false;
            var itemResult = default(ItemBase);

            var destroyPickup = false;
            
            player.Inventory.RemoveItem(item);
            player.ReferenceHub.inventory.ServerSendItems();

            for (var index = 0; index < outputs.Count; index++)
            {
                var output = outputs[index];

                if (output is Scp914AmmoOutput ammoOutput)
                {
                    player.Ammo.AddAmmo(output.Item, ammoOutput.Amount);
                }
                else
                {
                    if (ply.inventory.UserInventory.Items.Count < 8)
                    {
                        if (!itemUsed)
                        {
                            itemResult = player.Inventory.AddItem(output.Item, ItemAddReason.Scp914Upgrade, itemSerial);

                            if (itemResult != null)
                            {
                                if (itemHeld)
                                    ply.inventory.ServerSelectItem(itemSerial);

                                Scp914Recipes.PostProcessItem(player, itemResult, item, null, null, 
                                    entry, recipe, output, ref destroyPickup);

                                itemUsed = true;

                                resultItems.Add(itemResult);
                            }
                        }
                        else
                        {
                            var addedItem = player.Inventory.AddItem(output.Item, ItemAddReason.Scp914Upgrade);
                            
                            if (addedItem != null)
                            {
                                Scp914Recipes.PostProcessItem(player, addedItem, item, null, null, 
                                    entry, recipe, output, ref destroyPickup);
                                
                                resultItems.Add(addedItem);
                            }
                        }
                    }
                    else
                    {
                        var pickup = ExMap.SpawnItem(output.Item, position, Vector3.one,
                            ply.PlayerCameraReference.rotation);

                        if (pickup != null)
                        {
                            Scp914Recipes.PostProcessItem(player, null, item, pickup, null, entry, recipe, 
                                output, ref destroyPickup);
                            
                            resultPickups.Add(pickup);
                        }
                    }
                }
            }
            
            OnUpgraded.InvokeEvent(null,
                new Scp914Result(item, ListPool<ItemBase>.Shared.ToArrayReturn(resultItems),
                    ListPool<ItemPickupBase>.Shared.ToArrayReturn(resultPickups)), setting);
            
            Scp914Events.OnProcessedInventoryItem(new(item.ItemTypeId, item, setting, ply));
        }
        
        BodyArmorUtils.SetPlayerDirty(ply);
        
        Scp914Events.OnProcessedPlayer(new(position, setting, ply));
        
        HashSetPool<ushort>.Shared.Return(serials);
        ListPool<IScp914Output>.Shared.Return(outputs);
        
        return false;
    }
}