using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabExtended.API;
using LabExtended.API.CustomItems;
using LabExtended.Events;
using LabExtended.Events.Map;
using LabExtended.Utilities;
using UnityEngine;

namespace LabExtended.Patches.Events.Map;

/// <summary>
/// Implements the <see cref="ExMapEvents.PickupCollided"/> event.
/// </summary>
public static class PickupCollidedPatch
{
    /// <summary>
    /// Gets the invoker for the <see cref="CollisionDetectionPickup.OnCollided"/> event.
    /// </summary>
    public static FastEvent<Action<Collision>> OnCollided { get; } =
        FastEvents.DefineEvent<Action<Collision>>(typeof(CollisionDetectionPickup),
            nameof(CollisionDetectionPickup.OnCollided));
    
    [HarmonyPatch(typeof(CollisionDetectionPickup), nameof(CollisionDetectionPickup.ProcessCollision))]
    private static bool Prefix(CollisionDetectionPickup __instance, Collision collision)
    {
        var player = ExPlayer.Get(__instance.PreviousOwner);
        var args = new PickupCollidedEventArgs(player, __instance, collision);

        if (!ExMapEvents.OnPickupCollided(args))
            return false;

        OnCollided.InvokeEvent(__instance, collision);

        var magnitude = collision.relativeVelocity.sqrMagnitude;
        var weight = CustomItemUtils.GetPickupCustomWeight(__instance.Info.ItemId, __instance.Info.Serial, __instance.Info.WeightKg);
        var damage = weight * magnitude / 2f;

        if (damage > 15f && collision.collider.TryGetComponent<BreakableWindow>(out var window))
            window.Damage(damage * 0.4f, null, Vector3.zero);
        
        __instance.MakeCollisionSound(magnitude);
        return true;
    }
}