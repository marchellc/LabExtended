using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using Mirror;

using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerStatsSystem;

namespace LabExtended.Patches.Events
{
    public static class PlayerTogglingNoClipPatch
    {
        [HookPatch(typeof(PlayerTogglingNoClipArgs))]
        [HarmonyPatch(typeof(FpcNoclipToggleMessage), nameof(FpcNoclipToggleMessage.ProcessMessage))]
        public static bool Prefix(FpcNoclipToggleMessage __instance, NetworkConnection sender)
        {
            if (sender is null || !ExPlayer.TryGet(sender, out var player))
                return false;

            if (!player.IsNoClipPermitted)
                return false;

            var togglingArgs = new PlayerTogglingNoClipArgs(player, player.Stats.AdminFlags.HasFlag(AdminFlags.Noclip));

            if (!HookRunner.RunEvent(togglingArgs, true))
                return false;

            if (togglingArgs.NewState == togglingArgs.CurrentState)
                return false;

            player.Stats.AdminFlags.SetFlag(AdminFlags.Noclip, togglingArgs.NewState);
            return false;
        }
    }
}
