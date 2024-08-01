using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Scp939;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles;

using RelativePositioning;

using UnityEngine;

using Utils.Networking;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))]
    public static class Scp939LungePatch
    {
        public static bool Prefix(Scp939LungeAbility __instance, NetworkReader reader)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Switches.CanLungeAs939)
                return false;

            var pos = reader.ReadRelativePosition().Position;
            var hub = reader.ReadReferenceHub();
            var relative = reader.ReadRelativePosition();
            var target = ExPlayer.Get(hub);

            if (__instance.State != Scp939LungeState.Triggered)
            {
                if (!__instance.IsReady)
                    return false;

                var lungingArgs = new Scp939LungingArgs(scp, target, __instance.CastRole, __instance);

                if (!HookRunner.RunCancellable(lungingArgs, true))
                    return false;

                __instance.TriggerLunge();

                hub = lungingArgs.Target?.Hub;
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

                if (!Physics.Linecast(pos, curPos, PlayerRolesUtils.BlockerMask))
                    damageDealt = hub.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole, lungingArgs.LungeTargetDamage, Scp939DamageType.LungeTarget));

                var num = damageDealt ? 1f : 0f;

                if (!damageDealt || hub.IsAlive())
                {
                    fpcRole.FpcModule.Position = curPos;
                    hub.transform.rotation = curRot;
                }

                foreach (var other in ExPlayer.Players)
                {
                    if (other != target && HitboxIdentity.IsEnemy(__instance.Owner, other.Hub) && other.Role.Is<IFpcRole>(out var otherRole))
                    {
                        var otherPos = otherRole.FpcModule.Position;

                        if ((otherRole.FpcModule.Position - cachedPos).sqrMagnitude <= __instance._secondaryRangeSqr)
                        {
                            if (Physics.Linecast(otherPos, pos, PlayerRolesUtils.BlockerMask))
                                return false;

                            if (other.Hub.playerStats.DealDamage(new Scp939DamageHandler(__instance.CastRole, lungingArgs.LungeSecondaryDamage, Scp939DamageType.LungeSecondary)))
                            {
                                damageDealt = true;
                                num = Mathf.Max(num, 0.6f);
                            }
                        }
                    }
                }

                if (damageDealt)
                    Hitmarker.SendHitmarkerDirectly(__instance.Owner, num);

                __instance.State = Scp939LungeState.LandHit;
            }

            return false;
        }
    }
}