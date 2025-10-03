using Footprinting;

using HarmonyLib;

using InventorySystem.Items.ThrowableProjectiles;

using LabExtended.Events;
using LabExtended.Events.Map;

using UnityEngine;

namespace LabExtended.Patches.Events.Map
{
    /// <summary>
    /// Implements the <see cref="ProjectileActivatingEventArgs"/> event.
    /// </summary>
    public static class ProjectileActivatingPatch
    {
        [HarmonyPatch(typeof(TimedGrenadePickup), nameof(TimedGrenadePickup.OnExplosionDetected))]
        private static bool Prefix(TimedGrenadePickup __instance, Footprint attacker, Vector3 source, float range)
        {
            if (Vector3.Distance(__instance.transform.position, source) / range > 0.4f)
                return false;

            if (Physics.Linecast(__instance.transform.position, source, ThrownProjectile.HitBlockerMask))
                return false;

            if (!ExMapEvents.OnProjectileActivating(new(__instance, attacker, source)))
                return false;

            __instance._replaceNextFrame = true;
            __instance._attacker = attacker;

            return false;
        }
    }
}