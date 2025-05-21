using HarmonyLib;

using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.Ragdolls;
using PlayerRoles;

using PlayerStatsSystem;

using LabExtended.API;

using LabExtended.Events;
using LabExtended.Events.Scp049;

namespace LabExtended.Patches.Events.Scp049
{
    public static class Scp049AttemptingResurrectionPatch
    {
        [HarmonyPatch(typeof(Scp049ResurrectAbility), nameof(Scp049ResurrectAbility.CheckBeginConditions))]
        public static bool Prefix(Scp049ResurrectAbility __instance, BasicRagdoll ragdoll, ref Scp049ResurrectAbility.ResurrectError __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Toggles.CanUseResurrectAs049)
            {
                __result = Scp049ResurrectAbility.ResurrectError.Refused;
                return false;
            }

            __result = Scp049ResurrectAbility.ResurrectError.None;

            if (ragdoll.Info.RoleType is RoleTypeId.Scp0492)
            {
                if (ragdoll.Info.OwnerHub is null || !Scp049ResurrectAbility.DeadZombies.Contains(ragdoll.Info.OwnerHub.netId))
                    __result = Scp049ResurrectAbility.ResurrectError.TargetNull;
                else if (ragdoll.Info.Handler is null || ragdoll.Info.Handler is not AttackerDamageHandler)
                    __result = Scp049ResurrectAbility.ResurrectError.TargetInvalid;
            }
            else
            {
                var time = (ragdoll.Info.OwnerHub != null && __instance._senseAbility.DeadTargets.Contains(ragdoll.Info.OwnerHub)) ? 18f : 12f;

                if (ragdoll.Info.ExistenceTime >= time)
                    __result = Scp049ResurrectAbility.ResurrectError.Expired;
                else if (!ragdoll.Info.RoleType.IsHuman())
                    __result = Scp049ResurrectAbility.ResurrectError.TargetInvalid;
                else if (ragdoll.Info.OwnerHub is null || __instance.AnyConflicts(ragdoll))
                    __result = Scp049ResurrectAbility.ResurrectError.TargetNull;
                else if (!Scp049ResurrectAbility.IsSpawnableSpectator(ragdoll.Info.OwnerHub))
                    __result = Scp049ResurrectAbility.ResurrectError.TargetInvalid;
            }

            if (__result is Scp049ResurrectAbility.ResurrectError.None && ragdoll.Info.OwnerHub != null)
            {
                var ressurections = Scp049ResurrectAbility.GetResurrectionsNumber(ragdoll.Info.OwnerHub);

                if (ressurections < 2)
                    __result = Scp049ResurrectAbility.ResurrectError.None;
                else if (ressurections <= 2)
                    __result = Scp049ResurrectAbility.ResurrectError.MaxReached;
                else
                    __result = Scp049ResurrectAbility.ResurrectError.Refused;
            }

            var target = ExPlayer.Get(ragdoll.Info.OwnerHub);

            if (target != null && !target.Toggles.CanBeResurrectedBy049)
            {
                __result = Scp049ResurrectAbility.ResurrectError.TargetInvalid;
                return false;
            }

            var resurrectingArgs = new Scp049AttemptingResurrectionEventArgs(scp, target, __result);

            if (!ExScp049Events.OnAttemptingResurrection(resurrectingArgs))
            {
                __result = Scp049ResurrectAbility.ResurrectError.Refused;
                return false;
            }

            __result = resurrectingArgs.Error;
            return false;
        }
    }
}
