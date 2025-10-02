using CustomPlayerEffects;

using HarmonyLib;

using InventorySystem.Items.Pickups;
using InventorySystem.Searching;

using LabApi.Features.Wrappers;

using LabExtended.Extensions;

using Mirror;

using PlayerRoles.PlayableScps.Scp939.Ripples;

using RelativePositioning;

using UnityEngine;

using BodyArmorPickup = InventorySystem.Items.Armor.BodyArmorPickup;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Patches.Functions.Items;

/// <summary>
/// Implements custom weight from custom items.
/// </summary>
public static class PickupWeightPatches
{
    [HarmonyPatch(typeof(PickupRippleTrigger), nameof(PickupRippleTrigger.OnCollided))]
    private static bool RippleOnCollidedPrefix(PickupRippleTrigger __instance, CollisionDetectionPickup cdp,
        Collision collision)
    {
        if (!PickupRippleTrigger._anyInstances)
            return false;

        var magnitude = collision.relativeVelocity.sqrMagnitude;
        var weight = Mathf.Max(4f, ItemWeightPatches.GetWeight(cdp.Info.ItemId, cdp.Info.Serial, cdp.Info.WeightKg) 
                                   * PickupRippleTrigger.SoundRangeKg);
        var range = Mathf.Max(weight, cdp.GetRangeOfCollisionVelocity(magnitude));

        foreach (var trigger in PickupRippleTrigger.ActiveInstances)
        {
            if (!trigger._rateLimiter.RateReady)
                continue;

            var point = collision.GetContact(0).point;
            
            if ((point - trigger.CastRole.FpcModule.Position).sqrMagnitude < range * range)
            {
                trigger._rateLimiter.RegisterInput();
                trigger._syncPos = new RelativePosition(point);
                trigger.ServerSendRpcToObservers();
            }
        }

        return false;
    }
    
    [HarmonyPatch(typeof(Pickup), nameof(Pickup.Weight), MethodType.Getter)]
    private static bool LabApiWrapperPrefix(Pickup __instance, ref float __result)
    {
        __result = ItemWeightPatches.GetWeight(__instance.Type, __instance.Serial, __instance.Base.Info.WeightKg);
        return false;
    }
    
    [HarmonyPatch(typeof(PickupSyncInfoSerializer), nameof(PickupSyncInfoSerializer.WritePickupSyncInfo))]
    private static bool SyncInfoWritePrefix(NetworkWriter writer, PickupSyncInfo value)
    {
        writer.WriteSByte((sbyte)value.ItemId);
        writer.WriteUShort(value.Serial);
        writer.WriteFloat(ItemWeightPatches.GetWeight(value.ItemId, value.Serial, value.WeightKg));
        writer.WriteByte(value.SyncedFlags);

        return false;
    }
    
    [HarmonyPatch(typeof(PickupStandardPhysics), nameof(PickupStandardPhysics.UpdateWeight))]
    private static bool PhysicsUpdateWeightPrefix(PickupStandardPhysics __instance)
    {
        __instance.Rb.mass = Mathf.Max(0.001f,
            ItemWeightPatches.GetWeight(__instance.Pickup.Info.ItemId, __instance.Pickup.Info.Serial, __instance.Pickup.Info.WeightKg));
        return false;
    }
    
    [HarmonyPatch(typeof(ItemPickupBase), nameof(ItemPickupBase.SearchTimeForPlayer))]
    private static bool PickupSearchTimePrefix(ItemPickupBase __instance, ReferenceHub hub, ref float __result)
    {
        var weight = ItemWeightPatches.GetWeight(__instance.Info.ItemId, __instance.Info.Serial, __instance.Info.WeightKg);
        var time = ItemPickupBase.MinimalPickupTime + ItemPickupBase.WeightToTime * weight;

        for (var i = 0; i < hub.playerEffectsController.AllEffects.Length; i++)
        {
            if (hub.playerEffectsController.AllEffects[i] is not ISearchTimeModifier searchTimeModifier)
                continue;
            
            if (!(searchTimeModifier as StatusEffectBase).IsEnabled)
                continue;

            time = searchTimeModifier.ProcessSearchTime(time);
        }

        if (hub.inventory.CurInstance is ISearchTimeModifier curSearchTimeModifier)
            time = curSearchTimeModifier.ProcessSearchTime(time);

        __result = time;
        return false;
    }
    
    [HarmonyPatch(typeof(BodyArmorPickup), nameof(BodyArmorPickup.OnTriggerStay))]
    private static bool BodyArmorTriggerStayPrefix(BodyArmorPickup __instance, Collider other)
    {
        if (other.gameObject.layer != 9)
            return false;

        if (Vector3.Dot(Vector3.up, __instance.transform.right) > -0.8f)
            return false;

        if (!other.transform.root.TryGetComponent<ItemPickupBase>(out var pickup))
            return false;

        var weight = ItemWeightPatches.GetWeight(pickup.Info.ItemId, pickup.Info.Serial, pickup.Info.WeightKg);

        if (weight > 2.1f || !__instance._alreadyMovedPickups.Add(pickup.Info.Serial))
            return false;

        if (!pickup.Info.ItemId.TryGetItemPrefab(out var prefab))
            return false;

        if (prefab.Category is ItemCategory.Armor)
            return false;

        pickup.transform.position +=
            Vector3.up * ((__instance.transform.position.y - pickup.transform.position.y) * 2f + 0.16f);
        return false;
    }
}