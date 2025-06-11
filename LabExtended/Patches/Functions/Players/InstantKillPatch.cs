using HarmonyLib;

using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Containers;
using LabExtended.Utilities;

using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerStatsSystem;

namespace LabExtended.Patches.Functions.Players;

/// <summary>
/// Implements the <see cref="SwitchContainer.HasInstantKill"/> toggle.
/// </summary>
public static class InstantKillPatch
{
    private static FastEvent<Action<ReferenceHub, DamageHandlerBase>> OnAnyPlayerDamaged { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub, DamageHandlerBase>>(typeof(PlayerStats),
            nameof(PlayerStats.OnAnyPlayerDamaged));
    
    private static FastEvent<Action<ReferenceHub, DamageHandlerBase>> OnAnyPlayerDied { get; } =
        FastEvents.DefineEvent<Action<ReferenceHub, DamageHandlerBase>>(typeof(PlayerStats),
            nameof(PlayerStats.OnAnyPlayerDied));
    
    private static FastEvent<Action<DamageHandlerBase>> OnThisPlayerDamaged { get; } =
        FastEvents.DefineEvent<Action<DamageHandlerBase>>(typeof(PlayerStats),
            nameof(PlayerStats.OnThisPlayerDamaged));
    
    private static FastEvent<Action<DamageHandlerBase>> OnThisPlayerDied { get; } =
        FastEvents.DefineEvent<Action<DamageHandlerBase>>(typeof(PlayerStats),
            nameof(PlayerStats.OnThisPlayerDied));
    
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.DealDamage))]
    private static bool Prefix(PlayerStats __instance, DamageHandlerBase handler)
    {
        if (handler is not AttackerDamageHandler attackerDamageHandler ||
            !ExPlayer.TryGet(attackerDamageHandler.Attacker.Hub, out var attacker)
            || !attacker.Toggles.HasInstantKill)
            return true;

        if (__instance._hub.roleManager.CurrentRole is IDamageHandlerProcessingRole damageHandlerProcessingRole)
            handler = damageHandlerProcessingRole.ProcessDamageHandler(handler);

        var hurtingArgs = new PlayerHurtingEventArgs(attacker.ReferenceHub, __instance._hub, handler);
        
        PlayerEvents.OnHurting(hurtingArgs);

        if (!hurtingArgs.IsAllowed)
            return false;

        handler.ApplyDamage(__instance._hub);

        PlayerEvents.OnHurt(new(attacker.ReferenceHub, __instance._hub, handler));

        OnAnyPlayerDamaged.InvokeEvent(null, __instance._hub, handler);
        OnThisPlayerDamaged.InvokeEvent(null, handler);

        var dyingArgs = new PlayerDyingEventArgs(__instance._hub, attacker.ReferenceHub, handler);
        
        PlayerEvents.OnDying(dyingArgs);

        if (!dyingArgs.IsAllowed)
            return false;
        
        OnAnyPlayerDied.InvokeEvent(null, __instance._hub, handler);
        OnThisPlayerDied.InvokeEvent(null, handler);

        var role = __instance._hub.roleManager.CurrentRole.RoleTypeId;
        var position = __instance._hub.transform.position;
        var rotation = __instance._hub.transform.rotation;
        var velocity = __instance._hub.GetVelocity();
        
        __instance.KillPlayer(handler);
        
        PlayerEvents.OnDeath(new(__instance._hub, attacker.ReferenceHub, handler, role, position, velocity, rotation));
        return false;
    }
}