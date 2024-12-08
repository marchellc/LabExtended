using HarmonyLib;

using LabExtended.API;

using MapGeneration;

using PlayerRoles.FirstPersonControl;

using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.PlayableScps.Scp079.Rewards;

namespace LabExtended.Patches.Functions.Scp079Rewards
{
    public static class Scp079HidStoppedPatch
    {
        [HarmonyPatch(typeof(HidStoppedReward), nameof(HidStoppedReward.TryGrant))]
        public static bool Prefix(ReferenceHub ply)
        {
            if (!ExPlayer.TryGet(ply, out var player))
                return true;

            if (!player.Switches.CanCountAs079ExpTarget)
                return false;

            if (!player.Role.Is<IFpcRole>(out var fpcRole))
                return false;

            var playerPos = fpcRole.FpcModule.Position;
            var playerRoom = RoomIdUtils.RoomAtPositionRaycasts(playerPos);

            if (playerRoom is null)
                return false;

            if (!ExPlayer.Players.Any(x => x.Switches.CanCountAs079ExpTarget && HidStoppedReward.IsNearbyTeammate(playerPos, x.Hub)))
                return false;

            foreach (var role in Scp079Role.ActiveInstances)
            {
                if (Scp079RewardManager.CheckForRoomInteractions(role, playerRoom))
                {
                    HidStoppedReward._available = false;
                    Scp079RewardManager.GrantExp(role, 50, Scp079HudTranslation.ExpGainHidStopped);
                }
            }

            return false;
        }
    }
}