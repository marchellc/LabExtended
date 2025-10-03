using HarmonyLib;

using InventorySystem.Items.Pickups;
using InventorySystem.Items.ThrowableProjectiles;

using LabExtended.Events;
using LabExtended.Extensions;

using Mirror;

namespace LabExtended.Patches.Events.Map
{
    /// <summary>
    /// Implements the <see cref="ExMapEvents.ProjectileActivated"/> event.
    /// </summary>
    public static class ProjectileActivatedPatch
    {
        [HarmonyPatch(typeof(TimedGrenadePickup), nameof(TimedGrenadePickup.Update))]
        private static bool Prefix(TimedGrenadePickup __instance)
        {
            if (!__instance._replaceNextFrame)
                return false;

            if (!__instance.Info.ItemId.TryGetItemPrefab<ThrowableItem>(out var throwableItemTemplate)
                || throwableItemTemplate == null)
                return false;

            var thrownProjectile = UnityEngine.Object.Instantiate(throwableItemTemplate.Projectile);
            var thrownProjectilePhysics = thrownProjectile?.PhysicsModule as PickupStandardPhysics;

            if (thrownProjectile != null)
            {
                if (thrownProjectilePhysics != null && __instance.PhysicsModule is PickupStandardPhysics itemPhysics)
                {
                    var projectileRigidbody = thrownProjectilePhysics.Rb;
                    var itemRidigbody = itemPhysics.Rb;

                    projectileRigidbody.position = itemRidigbody.position;
                    projectileRigidbody.rotation = itemRidigbody.rotation;
                    projectileRigidbody.linearVelocity = itemRidigbody.linearVelocity;
                    projectileRigidbody.angularVelocity = itemRidigbody.angularVelocity;
                }

                __instance.Info.Locked = true;

                thrownProjectile.NetworkInfo = __instance.Info;
                thrownProjectile.PreviousOwner = __instance._attacker;

                if (ExMapEvents.OnProjectileActivated(new(__instance, thrownProjectile)))
                {
                    NetworkServer.Spawn(thrownProjectile.gameObject);

                    thrownProjectile.ServerActivate();
                }
                else
                {
                    UnityEngine.Object.Destroy(thrownProjectile.gameObject);
                }

                __instance.DestroySelf();
                __instance._replaceNextFrame = false;
            }

            return false;
        }
    }
}