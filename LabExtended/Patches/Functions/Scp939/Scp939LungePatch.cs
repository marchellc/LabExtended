using HarmonyLib;

using LabApi.Events.Arguments.Scp939Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Events;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles;

using RelativePositioning;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.Patches.Functions.Scp939;

/// <summary>
/// Implements the <see cref="ExScp939Events.Lunging"/> event.
/// </summary>
public static class Scp939LungePatch
{
    [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
    private static bool Prefix(Scp939LungeAbility __instance, NetworkReader reader)
    {
        var readRelative = reader.ReadRelativePosition();
        var readPosition = readRelative.Position;
        var readTarget = reader.ReadReferenceHub();
        var readTargetRelative = reader.ReadRelativePosition();
        
        if (!ExPlayer.TryGet(__instance.Owner, out var scp))
	        return false;

        if (!scp.Toggles.CanLungeAs939)
	        return false;

        if (__instance.State != Scp939LungeState.Triggered)
        {
	        if (!__instance.IsReady)
		        return false;
	        
	        __instance.TriggerLunge();
        }

        if (readTarget == null || !HitboxIdentity.IsEnemy(__instance.Owner, readTarget) ||
            readTarget.roleManager.CurrentRole is not FpcStandardRoleBase { FpcModule: var fpcModule })
	        return false;

        using (new FpcBacktracker(readTarget, readTargetRelative.Position))
        {
	        using (new FpcBacktracker(__instance.Owner, fpcModule.Position, Quaternion.identity))
	        {
		        var vector = fpcModule.Position - __instance.CastRole.FpcModule.Position;

		        if (vector.SqrMagnitudeIgnoreY() > __instance._overallTolerance * __instance._overallTolerance ||
		            vector.y > __instance._overallTolerance || vector.y < 0f - __instance._bottomTolerance)
			        return false;
	        }
        }

        using (new FpcBacktracker(__instance.Owner, readPosition, Quaternion.identity))
	        readPosition = __instance.CastRole.FpcModule.Position;

        var targetTransform = readTarget.transform;
        var targetPosition = fpcModule.Position;
        var targetRotation = targetTransform.rotation;
        var targetVector = new Vector3(readPosition.x, targetPosition.y, readPosition.z);

        targetTransform.forward = -__instance.Owner.transform.forward;
        fpcModule.Position = targetVector;

        var damageDealt = false;

        if (!Physics.Linecast(readPosition, targetVector, PlayerRolesUtils.AttackMask))
        {
	        var attackingArgs = new Scp939AttackingEventArgs(__instance.Owner, readTarget, Scp939LungeAbility.LungeDamage);
	        
	        Scp939Events.OnAttacking(attackingArgs);

	        if (!attackingArgs.IsAllowed)
		        return false;

	        readTarget = attackingArgs.Target.ReferenceHub;
	        damageDealt =
		        readTarget.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole, attackingArgs.Damage,
			        Scp939DamageType.LungeTarget));
	        
	        Scp939Events.OnAttacked(new(__instance.Owner, readTarget, attackingArgs.Damage));
        }

        var hitmarkerSize = damageDealt ? Scp939LungeAbility.MainHitmarkerSize : 0f;

        if (!damageDealt || readTarget.IsAlive())
        {
	        fpcModule.Position = targetVector;
	        targetTransform.rotation = targetRotation;
        }

        foreach (var hub in ReferenceHub.AllHubs)
        {
	        if (hub == readTarget || !HitboxIdentity.IsEnemy(__instance.Owner, hub) || hub.roleManager.CurrentRole is not FpcStandardRoleBase secondaryTargetRole)
		        continue;

	        var secondaryPosition = secondaryTargetRole.FpcModule.Position;
	        
	        if ((secondaryTargetRole.FpcModule.Position - targetVector).sqrMagnitude > __instance._secondaryRangeSqr)
		        continue;

	        if (Physics.Linecast(secondaryPosition, readPosition, PlayerRolesUtils.AttackMask))
		        return false;
	        
	        var secondaryAttackingArgs = new Scp939AttackingEventArgs(__instance.Owner, hub, Scp939LungeAbility.SecondaryDamage);
	        
	        Scp939Events.OnAttacking(secondaryAttackingArgs);

	        if (!secondaryAttackingArgs.IsAllowed)
		        return false;

	        if (secondaryAttackingArgs.Target.ReferenceHub.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole, 
		            secondaryAttackingArgs.Damage, Scp939DamageType.LungeSecondary)))
	        {
		        Scp939Events.OnAttacked(new(__instance.Owner, secondaryAttackingArgs.Target.ReferenceHub, secondaryAttackingArgs.Damage));

		        damageDealt = true;

		        hitmarkerSize = Mathf.Max(hitmarkerSize, Scp939LungeAbility.SecondaryHitmarkerSize);
	        }
        }
        
        if (damageDealt)
	        Hitmarker.SendHitmarkerDirectly(__instance.Owner, hitmarkerSize);

        __instance.State = Scp939LungeState.LandHit;
        return false;
    }
}