﻿using HarmonyLib;

using LabExtended.API;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;

using UnityEngine;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(Scp939VisibilityController), nameof(Scp939VisibilityController.ValidateVisibility))]
    public static class Scp939VisibilityPatch
    {
        public static bool Prefix(Scp939VisibilityController __instance, ReferenceHub hub, ref bool __result)
        {
            if (!BaseValidateVisibility(hub, __instance.Owner, __instance.IgnoredFlags))
                return __result = false;

            if (!HitboxIdentity.IsEnemy(__instance.Owner, hub))
            {
                __result = true;
                return false;
            }

            if (!ExPlayer.TryGet(__instance.Owner, out var scp) || !ExPlayer.TryGet(hub, out var target))
                return true;

            if (!target.Role.Is<FpcStandardRoleBase>(out var fpcRole) || AlphaWarheadController.Detonated)
            {
                __result = true;
                return false;
            }

            var module = __instance._scpRole.FpcModule;
            var range = __instance.BaseRangeForPlayer(hub, fpcRole);

            range = Mathf.Max(range, __instance.DetectionRangeForPlayer(hub));
            range += module.MaxMovementSpeed * __instance._pingTolerance;

            var inRange = (fpcRole.FpcModule.Position - module.Position).sqrMagnitude <= (range * range);
            var isSeen = scp.Switches.CanSeeEveryoneAs939 || target.Switches.IsVisibleToScp939 || inRange || (Scp939VisibilityController.LastSeen.TryGetValue(hub.netId, out var lastSeenInfo) && lastSeenInfo.Elapsed < __instance._sustain);

            if (!inRange || __instance._scpRole.IsLocalPlayer)
            {
                __result = isSeen;
                return false;
            }

            Scp939VisibilityController.LastSeen[hub.netId] = new Scp939VisibilityController.LastSeenInfo() { Time = NetworkTime.time };

            __result = true;
            return false;
        }

        private static bool BaseValidateVisibility(ReferenceHub hub, ReferenceHub owner, InvisibilityFlags ignoredFlags)
        {
            var role = hub.roleManager.CurrentRole as ICustomVisibilityRole;

            return role is null || (role.VisibilityController.GetActiveFlags(owner) & ~ignoredFlags) == InvisibilityFlags.None;
        }
    }
}