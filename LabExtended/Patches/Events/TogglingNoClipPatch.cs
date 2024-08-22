using HarmonyLib;

using LabExtended.API;
using LabExtended.Core.Hooking;
using LabExtended.Events.Player;

using Mirror;

using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerStatsSystem;

namespace LabExtended.Patches.Events
{
    [HarmonyPatch(typeof(FpcNoclipToggleMessage), nameof(FpcNoclipToggleMessage.ProcessMessage))]
    public static class TogglingNoClipPatch
    {
        public static bool Prefix(FpcNoclipToggleMessage __instance, NetworkConnection sender)
        {
            if (sender is null || !ExPlayer.TryGet(sender, out var player))
                return false;

            if (!player.IsNoclipPermitted)
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
