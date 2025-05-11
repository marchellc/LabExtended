using HarmonyLib;

using InventorySystem.Items.Pickups;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Map;

using UnityEngine;

namespace LabExtended.Patches.Events.Map;

/// <summary>
/// Implements the <see cref="ExMapEvents.PickupCollided"/> event.
/// </summary>
public static class PickupCollidedPatch
{
    [HarmonyPatch(typeof(CollisionDetectionPickup), nameof(CollisionDetectionPickup.OnCollisionEnter))]
    private static bool Prefix(CollisionDetectionPickup __instance, Collision collision)
    {
        var player = ExPlayer.Get(__instance.PreviousOwner);
        var args = new PickupCollidedEventArgs(player, __instance, collision);

        if (!ExMapEvents.OnPickupCollided(args))
            return false;

        return true;
    }
}