using Common.Extensions;
using HarmonyLib;

using LabExtended.API;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.Round;
using LabExtended.API.Voice;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.Patches.Functions
{
    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class PlayerLeavePatch
    {
        public static event Action<ExPlayer> OnLeaving;

        public static bool Prefix(ReferenceHub __instance)
        {
            try
            {
                if (__instance.isLocalPlayer)
                    return true;

                var player = ExPlayer.Get(__instance);

                if (player is null)
                    return true;

                OnLeaving.Call(player);

                if (ExRound.DisableRoundLockOnLeave && ExRound.RoundLock.HasValue && ExRound.RoundLock.Value.EnabledBy == player)
                    ExRound.IsRoundLocked = false;

                if (ExRound.DisableLobbyLockOnLeave && ExRound.LobbyLock.HasValue && ExRound.LobbyLock.Value.EnabledBy == player)
                    ExRound.IsLobbyLocked = false;

                if (player._voiceProfile != null)
                    VoiceSystem.SetProfile(player, null);

                GhostModePatch.GhostedPlayers.Remove(player.NetId);
                GhostModePatch.GhostedTo.Remove(player.NetId);

                RemoteAdminUtils.ClearWhitelisted(player.NetId);

                foreach (var pair in GhostModePatch.GhostedTo)
                    pair.Value.Remove(player.NetId);

                foreach (var helper in PlayerListHelper._handlers)
                    helper.Remove(player.NetId);

                if (!player.IsNpc)
                {
                    ExPlayer._players.Remove(player);
                    ExLoader.Info("Extended API", $"Player &3{player.Name}&r (&3{player.UserId}&r) &1left&r from &3{player.Address}&r!");
                }
                else
                {
                    if (!player.NpcHandler._isDestroying)
                        player.NpcHandler.Destroy();

                    ExPlayer._npcPlayers.Remove(player);
                }

                return true;
            }
            catch (Exception ex)
            {
                ExLoader.Error("Extended API", $"An error occured while handling player leave!\n{ex.ToColoredString()}");
                return true;
            }
        }
    }
}