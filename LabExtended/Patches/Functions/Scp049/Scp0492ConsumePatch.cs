using HarmonyLib;

using LabApi.Events.Arguments.Scp0492Events;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Events;
using LabExtended.Events.Scp0492;

using PlayerRoles;
using PlayerRoles.Ragdolls;
using PlayerRoles.PlayableScps.Scp049.Zombies;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp049
{
    public static class Scp0492ConsumePatch
    {
        [EventPatch(typeof(Scp0492ConsumingRagdollEventArgs), true)]
        [HarmonyPatch(typeof(ZombieConsumeAbility), nameof(ZombieConsumeAbility.ServerValidateBegin))]
        public static bool Prefix(ZombieConsumeAbility __instance, BasicRagdoll ragdoll, ref byte __result)
        {
            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            var owner = ExPlayer.Get(ragdoll.Info.OwnerHub);
            var error = ZombieConsumeAbility.ConsumeError.None;

            if (!scp.Toggles.CanConsumeRagdollsAsZombie)
            {
                error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
            }
            else if (owner is { Toggles.CanBeConsumedByZombies: false })
            {
                error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
            }
            else if (ZombieConsumeAbility.ConsumedRagdolls.Contains(ragdoll))
            {
                error = ZombieConsumeAbility.ConsumeError.AlreadyConsumed;
            }
            else if (!ragdoll.Info.RoleType.IsHuman() || !__instance.ServerValidateAny())
            {
                error = ZombieConsumeAbility.ConsumeError.TargetNotValid;
            }
            else if (Mathf.Approximately(scp.Stats.Health.NormalizedValue, 1f))
            {
                error = ZombieConsumeAbility.ConsumeError.FullHealth;
            }
            else if (ZombieConsumeAbility.AllAbilities.Any(a => a.IsInProgress 
                                                                && a.CurRagdoll == ragdoll))
            {
                error = ZombieConsumeAbility.ConsumeError.BeingConsumed;
            }

            var consumingEventArgs = new Scp0492ConsumingRagdollEventArgs(scp, owner, ragdoll, error);

            if (!ExScp0492Events.OnConsumingRagdoll(consumingEventArgs))
            {
                __result = (byte)consumingEventArgs.Error;
                return false;
            }

            var labConsumingEventArgs = new Scp0492StartingConsumingCorpseEventArgs(scp.ReferenceHub, ragdoll, consumingEventArgs.Error);
            
            Scp0492Events.OnStartingConsumingCorpse(labConsumingEventArgs);

            if (!labConsumingEventArgs.IsAllowed)
            {
                __result = (byte)labConsumingEventArgs.Error;
                return false;
            }

            __result = (byte)labConsumingEventArgs.Error;
            return false;
        }
    }
}