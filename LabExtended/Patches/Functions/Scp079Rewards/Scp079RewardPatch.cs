﻿using HarmonyLib;

using LabExtended.API;

using PlayerRoles;
using PlayerRoles.PlayableScps.Scp079;

namespace LabExtended.Patches.Functions.Scp079Rewards
{
    public static class Scp079RewardPatch
    {
        [HarmonyPatch(typeof(Scp079TierManager), nameof(Scp079TierManager.ServerGrantExperience))]
        public static bool Prefix(Scp079TierManager __instance, int amount, Scp079HudTranslation reason, RoleTypeId subject)
        {
            if (amount <= 0)
                return false;

            if (!ExPlayer.TryGet(__instance.Owner, out var scp))
                return true;

            if (!scp.Toggles.CanGainExpAs079)
                return false;

            __instance._expGainQueue.Enqueue(new Scp079TierManager.ExpQueuedNotification(amount, reason, subject));
            __instance.TotalExp += amount;

            return false;
        }
    }
}