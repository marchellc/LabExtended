using HarmonyLib;

using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using LabExtended.API;
using LabExtended.API.CustomFirearms;
using LabExtended.API.CustomItems;
using LabExtended.Events;
using LabExtended.Events.Firearms;
using LabExtended.Events.Player;
using LabExtended.Extensions;
using PlayerRoles;
using PlayerStatsSystem;

using UnityEngine;

namespace LabExtended.Patches.Events.Player;

/// <summary>
/// Implements the <see cref="FirearmRayCastEventArgs"/>, <see cref="PlayerShootingFirearmEventArgs"/> and <see cref="PlayerShotFirearmEventArgs"/> events.
/// </summary>
public static class PlayerShootingFirearmPatches
{
    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerAppendPrescan))]
    public static bool HitscanPrefix(HitscanHitregModuleBase __instance, Ray targetRay, HitscanResult toAppend)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;

        var distance = __instance.DamageFalloffDistance + __instance.FullDamageDistance;

        if (!Physics.Raycast(targetRay, out var hit, distance, HitscanHitregModuleBase.HitregMask))
            return false;

        var custom = CustomItemManager.InventoryItems.GetValue<CustomFirearmInstance>(__instance.Firearm);
        var args = new FirearmRayCastEventArgs(player, __instance.Firearm, targetRay, distance, hit);

        custom?.OnRayCast(args);
        
        if (!ExFirearmEvents.OnRayCast(args)  || !args.Hit.HasValue)
            return false;

        hit = args.Hit.Value;
        
        if (hit.collider is null)
            return false;

        if (!hit.collider.TryGetComponent<IDestructible>(out var destructible))
        {
            toAppend.Obstacles.Add(new(targetRay, hit));
        }
        else
        {
            if (!__instance.ValidateTarget(destructible, toAppend))
                return false;
            
            toAppend.Destructibles.Add(new(destructible, hit, targetRay));
        }
        
        return false;
    }

    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyDestructibleDamage))]
    public static bool HitscanProcessTargetHitPrefix(HitscanHitregModuleBase __instance, DestructibleHitPair target, HitscanResult result)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;

        var damage = __instance.DamageAtDistance(target.Hit.distance);
        var args = new PlayerShootingFirearmEventArgs(player, __instance.Firearm, target.Destructible, target.Ray,
            target.Hit, damage, result);

        if (!ExPlayerEvents.OnShootingFirearm(args))
            return false;
        
        var handler = __instance.GetHandler(args.TargetDamage);

        if (target.Destructible is HitboxIdentity hitboxIdentity
            && !hitboxIdentity.TargetHub.IsAlive())
        {
            result.RegisterDamage(hitboxIdentity, damage, handler);

            ExPlayerEvents.OnShotFirearm(new(player, __instance.Firearm, target.Destructible, target.Ray, target.Hit,
                args.TargetDamage, result, args.TargetPlayer));
            return false;
        }

        if (!target.Destructible.Damage(args.TargetDamage, handler, target.Hit.point))
            return false;
        
        result.RegisterDamage(target.Destructible, damage, handler);
        
        ExPlayerEvents.OnShotFirearm(new(player, __instance.Firearm, target.Destructible, target.Ray, target.Hit,
            args.TargetDamage, result, args.TargetPlayer));
        
        __instance.ServerPlayImpactEffects(target.Raycast, damage > 0f);
        return false;
    }

    [HarmonyPatch(typeof(HitscanHitregModuleBase), nameof(HitscanHitregModuleBase.ServerApplyObstacleDamage))]
    public static bool HitscanProcessObstacleHitPrefix(DisruptorHitregModule __instance, HitRayPair hitInfo, HitscanResult result)
    {
        if (!ExPlayer.TryGet(__instance.Firearm.Owner, out var player))
            return false;
        
        var args = new PlayerShootingFirearmEventArgs(player, __instance.Firearm, null, hitInfo.Ray, hitInfo.Hit, 
            0f, result);

        if (!ExPlayerEvents.OnShootingFirearm(args))
            return false;
        
        __instance.ServerPlayImpactEffects(hitInfo, false);

        ExPlayerEvents.OnShotFirearm(new(player, __instance.Firearm, null, hitInfo.Ray, hitInfo.Hit, 0f, result,
            args.TargetPlayer));
        return false;
    }
}