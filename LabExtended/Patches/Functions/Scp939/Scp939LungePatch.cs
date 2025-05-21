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

using Scp939LungingEventArgs = LabExtended.Events.Scp939.Scp939LungingEventArgs;

namespace LabExtended.Patches.Functions.Scp939
{
    public static class Scp939LungePatch
    {
        [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
        public static bool Prefix(Scp939LungeAbility __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Toggles.CanLungeAs939)
                return false;

            var pos = reader.ReadRelativePosition().Position;
            var hub = reader.ReadReferenceHub();
            var relative = reader.ReadRelativePosition();
            var target = ExPlayer.Get(hub);

            var lungingArgs = new Scp939LungingEventArgs(scp, target, __instance.CastRole, __instance);

            if (!ExScp939Events.OnLunging(lungingArgs))
                return false;

            if (__instance.State != Scp939LungeState.Triggered)
            {
                if (!__instance.IsReady)
                    return false;

                __instance.TriggerLunge();
            }

            hub = lungingArgs.Target?.ReferenceHub;
            target = lungingArgs.Target;

            if (hub is null || !HitboxIdentity.IsEnemy(__instance.Owner, hub))
                return false;

            if (!target.Role.Is<IFpcRole>(out var fpcRole))
                return false;

            using (new FpcBacktracker(hub, relative.Position))
            using (new FpcBacktracker(__instance.Owner, fpcRole.FpcModule.Position, Quaternion.identity))
            {
                var direction = fpcRole.FpcModule.Position - __instance.CastRole.FpcModule.Position;

                if (direction.SqrMagnitudeIgnoreY() > __instance._overallTolerance * __instance._overallTolerance)
                    return false;

                if (direction.y > __instance._overallTolerance || direction.y < -__instance._overallTolerance)
                    return false;
            }

            using (new FpcBacktracker(__instance.Owner, pos, Quaternion.identity))
                pos = __instance.CastRole.FpcModule.Position;

            var curPos = fpcRole.FpcModule.Position;
            var curRot = hub.transform.rotation;

            var cachedPos = new Vector3(pos.x, pos.y, pos.z);

            hub.transform.forward = -__instance.Owner.transform.forward;
            fpcRole.FpcModule.Position = cachedPos;

            var damageDealt = false;

            if (!Physics.Linecast(pos, curPos, PlayerRolesUtils.AttackMask))
            {
                var attackingArgs = new Scp939AttackingEventArgs(__instance.Owner, target.ReferenceHub, lungingArgs.LungeTargetDamage);
                
                Scp939Events.OnAttacking(attackingArgs);

                if (!attackingArgs.IsAllowed)
                    return false;
                
                damageDealt = hub.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole,
                    attackingArgs.Damage, Scp939DamageType.LungeTarget));
                
                Scp939Events.OnAttacked(new(__instance.Owner, target.ReferenceHub, attackingArgs.Damage));
            }

            var num = damageDealt ? 1f : 0f;

            if (!damageDealt || hub.IsAlive())
            {
                fpcRole.FpcModule.Position = curPos;
                hub.transform.rotation = curRot;
            }

            foreach (var other in ExPlayer.Players)
            {
                if (other != target && HitboxIdentity.IsEnemy(__instance.Owner, other.ReferenceHub) 
                                    && other.Role.Is<IFpcRole>(out var otherRole))
                {
                    var otherPos = otherRole.FpcModule.Position;

                    if ((otherRole.FpcModule.Position - cachedPos).sqrMagnitude <= __instance._secondaryRangeSqr)
                    {
                        if (Physics.Linecast(otherPos, pos, PlayerRolesUtils.AttackMask))
                            return false;

                        var attackingArgs = new Scp939AttackingEventArgs(__instance.Owner, other.ReferenceHub,
                            lungingArgs.LungeSecondaryDamage);
                        
                        Scp939Events.OnAttacking(attackingArgs);
                        
                        if (!attackingArgs.IsAllowed)
                            continue;

                        if (other.ReferenceHub.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole, 
                                attackingArgs.Damage, Scp939DamageType.LungeSecondary)))
                        {
                            Scp939Events.OnAttacked(new(__instance.Owner, target.ReferenceHub, attackingArgs.Damage));
                            
                            damageDealt = true;
                            num = Mathf.Max(num, 0.6f);
                        }
                    }
                }
            }

            if (damageDealt)
                Hitmarker.SendHitmarkerDirectly(__instance.Owner, num);

            __instance.State = Scp939LungeState.LandHit;
            return false;
        }
    }
}