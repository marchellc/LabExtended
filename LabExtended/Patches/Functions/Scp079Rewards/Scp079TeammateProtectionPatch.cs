using HarmonyLib;
using LabExtended.API;

using Mirror;

using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp079.Rewards;

using UnityEngine;

namespace LabExtended.Patches.Functions.Scp079Rewards
{
    [HarmonyPatch(typeof(TeammateProtectionRewards.TrackedTeammate), nameof(TeammateProtectionRewards.TrackedTeammate.GetAttackersNonAlloc))]
    public static class Scp079TeammateProtectionPatch
    {
        public static bool Prefix(TeammateProtectionRewards.TrackedTeammate __instance, out Vector3[] attackersPositions, ref int __result)
        {
            attackersPositions = TeammateProtectionRewards.TrackedTeammate.AttackersNonAlloc;

            if (NetworkTime.time > __instance._lastDamageTime || __instance._damageReceived < 100f)
            {
                __result = 0;
                return false;
            }

            var index = 0;

            foreach (var pair in __instance._attackers)
            {
                if (pair.Value <= __instance._lastDamageTime && ExPlayer.TryGet(pair.Key, out var player)
                    && player.Switches.CanCountAs079ExpTarget && player.Role.Is<IFpcRole>(out var fpcRole))
                {
                    TeammateProtectionRewards.TrackedTeammate.AttackersNonAlloc[index] = fpcRole.FpcModule.Position;

                    if (++index >= 5)
                        break;
                }
            }

            __instance._attackers.Clear();
            __instance._damageReceived = 0;

            __result = index;
            return false;
        }
    }
}