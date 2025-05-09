using HarmonyLib;

using InventorySystem.Items;
using InventorySystem.Items.Pickups;

using LabApi.Events.Arguments.Scp914Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Scp914;
using LabExtended.API.Scp914.Interfaces;

using LabExtended.Core;
using LabExtended.Utilities;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using Scp914;

namespace LabExtended.Patches.Functions.Scp914;

/// <summary>
/// Implements custom recipes for pickups.
/// </summary>
public static class UpgradePickupPatch
{
    [HarmonyPatch(typeof(Scp914Upgrader), nameof(Scp914Upgrader.ProcessPickup))]
    private static bool Prefix(ItemPickupBase pickup, bool upgradeDropped, Scp914KnobSetting setting)
    {
        if (!ApiLoader.ApiConfig.OtherSection.Scp914CustomRecipes)
            return true;
        
        if (pickup.Info.Locked)
            return false;

        if (!upgradeDropped)
            return false;

        var player = ExPlayer.Get(pickup.PreviousOwner);
        var position = pickup.transform.position + Scp914Controller.MoveVector;
        var processingArgs = new Scp914ProcessingPickupEventArgs(position, setting, pickup);
        
        Scp914Events.OnProcessingPickup(processingArgs);

        if (!processingArgs.IsAllowed)
            return false;

        position = processingArgs.NewPosition;
        setting = processingArgs.KnobSetting;

        if (!Scp914Recipes.TryGetEntry(setting, pickup.Info.ItemId, out var entry))
        {
            ApiLog.Debug("SCP-914 API", $"No recipe entry found for {pickup.Info.ItemId}");
            return false;
        }

        var outputs = ListPool<IScp914Output>.Shared.Rent();
        var recipe = entry.Recipes.GetRandomWeighted(x => x.Chance);

        if (recipe is null)
        {
            ApiLog.Debug("SCP-914 API", $"Selected a null recipe");
            return false;
        }

        var pickupRotation = pickup.Rotation;
        var pickupScale = pickup.transform.localScale;
        var pickupSerial = pickup.Info.Serial;
        var pickupUsed = false;
        var pickupDestroy = true;
        
        recipe.Pick(player, outputs);
        
        var resultingItems = ListPool<ItemBase>.Shared.Rent();
        var resultingPickups = ListPool<ItemPickupBase>.Shared.Rent();

        for (var index = 0; index < outputs.Count; index++)
        {
            var output = outputs[index];
            var resultPickup = ExMap.SpawnItem<ItemPickupBase>(output.Item, position, pickupScale, pickupRotation,
                pickupUsed ? pickupSerial : null);

            if (resultPickup != null)
            {
                if (player != null)
                    resultPickup.PreviousOwner = player.Footprint;

                if (resultPickup is IUpgradeTrigger upgradeTrigger)
                    upgradeTrigger.ServerOnUpgraded(setting);

                pickupUsed = true;

                Scp914Recipes.PostProcessItem(player, null, null, resultPickup, pickup, entry, recipe, output,
                    ref pickupDestroy);

                resultingPickups.Add(resultPickup);
            }
        }

        if (pickupDestroy)
            pickup.DestroySelf();

        UpgradeItemPatch.OnUpgraded.InvokeEvent(null,
            new Scp914Result(pickup, ListPool<ItemBase>.Shared.ToArrayReturn(resultingItems),
                ListPool<ItemPickupBase>.Shared.ToArrayReturn(resultingPickups)));
        
        ListPool<IScp914Output>.Shared.Return(outputs);
        
        Scp914Events.OnProcessedPickup(new(pickup.Info.ItemId, position, setting, pickup));
        return false;
    }
}