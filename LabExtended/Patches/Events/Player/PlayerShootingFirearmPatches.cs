using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Firearms;
using LabExtended.Events.Player;

using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="FirearmRayCastEventArgs"/>, <see cref="PlayerShootingFirearmEventArgs"/> and <see cref="PlayerShotFirearmEventArgs"/> events.
/// </summary>
public static class PlayerShootingFirearmPatches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerPerformHitscan))]
    public static bool HitscanPrefix(HitscanHitregModuleBase __instance, Ray targetRay, out float targetDamage,
        ref bool __result)
    {
        targetDamage = 0f;

        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;

        __instance.ServerLastDamagedTargets.Clear();

        var maxDistance = __instance.DamageFalloffDistance + __instance.FullDamageDistance;
        var wasHit = Physics.Raycast(targetRay, out var raycastHit, maxDistance, HitscanHitregModuleBase.HitregMask);

        var rayCastEventArgs = new FirearmRayCastEventArgs(player, __instance.Firearm, targetRay, maxDistance,
            wasHit ? raycastHit : null);

        if (!ExFirearmEvents.OnRayCast(rayCastEventArgs)
            || (wasHit && !rayCastEventArgs.Hit.HasValue)
            || !wasHit)
            return __result = false;

        if (rayCastEventArgs.Hit.HasValue)
            raycastHit = rayCastEventArgs.Hit.Value;

        if (raycastHit.collider != null)
        {
            if (raycastHit.collider.TryGetComponent<IDestructible>(out var destructible))
            {
                if (!__instance.ValidateTarget(destructible))
                    return __result = false;

                targetDamage = __instance.ServerProcessTargetHit(destructible, raycastHit);
            }
            else
            {
                var shootingEventArgs =
                    new PlayerShootingFirearmEventArgs(player, __instance.Firearm, null, targetDamage, raycastHit);

                if (!ExPlayerEvents.OnShootingFirearm(shootingEventArgs))
                    return __result = false;
                
                targetDamage = __instance.ServerProcessObstacleHit(raycastHit);
                
                ExPlayerEvents.OnShotFirearm(new(player, __instance.Firearm, null, targetDamage, raycastHit,
                    shootingEventArgs.TargetPlayer));
            }

            if (__instance.Firearm.TryGetModule<ImpactEffectsModule>(out var impactEffectsModule))
                impactEffectsModule.ServerProcessHit(raycastHit, targetRay.origin, targetDamage > 0f);

            __result = true;
        }

        return false;
    }

    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerProcessTargetHit))]
    public static bool HitscanProcessTargetHitPrefix(HitscanHitregModuleBase __instance, IDestructible dest,
        RaycastHit hitInfo, ref float __result)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;
        
        __result = __instance.DamageAtDistance(hitInfo.distance);

        var shootingEventArgs =
            new PlayerShootingFirearmEventArgs(player, __instance.Firearm, dest, __result, hitInfo);

        if (!ExPlayerEvents.OnShootingFirearm(shootingEventArgs))
        {
            __result = shootingEventArgs.TargetDamage;
            return false;
        }

        __result = shootingEventArgs.TargetDamage;

        var handler = new FirearmDamageHandler(__instance.Firearm, shootingEventArgs.TargetDamage,
            __instance.EffectivePenetration, __instance.UseHitboxMultipliers);

        if (!dest.Damage(shootingEventArgs.TargetDamage, handler, hitInfo.point))
        {
            __result = 0f;
            return false;
        }
        
        if (dest is HitboxIdentity hitboxIdentity)
            __instance.SendDamageIndicator(hitboxIdentity.TargetHub, shootingEventArgs.TargetDamage);

        __instance.ServerLastDamagedTargets.Add(dest);
        
        ExPlayerEvents.OnShotFirearm(new(player, __instance.Firearm, dest, handler.DealtHealthDamage, hitInfo,
            shootingEventArgs.TargetPlayer));
        return false;
    }

    [HarmonyPatch(typeof(DisruptorHitregModule), nameof(DisruptorHitregModule.ServerProcessTargetHit))]
    public static bool DisruptorProcessTargetHitPrefix(DisruptorHitregModule __instance, IDestructible dest,
        RaycastHit hitInfo, ref float __result)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;
        
        __result = __instance.DamageAtDistance(hitInfo.distance) 
                   * Mathf.Pow(1f / __instance._singleShotDivisionPerTarget, __instance._serverPenetrations);

        var shootingEventArgs =
            new PlayerShootingFirearmEventArgs(player, __instance.Firearm, dest, __result, hitInfo);

        if (!ExPlayerEvents.OnShootingFirearm(shootingEventArgs))
        {
            __result = 0f;
            return false;
        }

        __result = shootingEventArgs.TargetDamage;

        var handler =
            new DisruptorDamageHandler(__instance.DisruptorShotData, __instance._lastShotRay.direction, __result);

        if (!dest.Damage(__result, handler, hitInfo.point))
        {
            __result = 0f;
            return false;
        }
        
        if (dest is HitboxIdentity hitboxIdentity)
            __instance.SendDamageIndicator(hitboxIdentity.TargetHub, __result);

        __instance.ServerLastDamagedTargets.Add(dest);
        return false;
    }
}