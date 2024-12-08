using HarmonyLib;

using LabExtended.API;
using LabExtended.Attributes;
using LabExtended.Core;
using LabExtended.Core.Hooking;

using LabExtended.Events.Player;
using LabExtended.Extensions;

using RemoteAdmin;

namespace LabExtended.Patches.Events
{
    public static class SendingAdminChatMessagePatch
    {
        [HookPatch(typeof(PlayerSendingAdminChatMessageArgs))]
        [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessAdminChat))]
        public static bool Prefix(string q, CommandSender sender)
        {
            if (!CommandProcessor.CheckPermissions(sender, "Admin Chat", PlayerPermissions.AdminChat, string.Empty, true))
            {
                sender.RaReply("You don't have permission to access Admin Chat!", false, true, string.Empty);
                return false;
            }

            if (!ExPlayer.TryGet(sender, out var player))
                return true;

            q = Misc.SanitizeRichText(q, "[", "]");

            if (string.IsNullOrWhiteSpace(q.Remove("@")))
                return false;

            if (q.Length >= 2000)
                q = q.SubstringPostfix(2000, "...");

            var sendingArgs = new PlayerSendingAdminChatMessageArgs(player, q);

            if (!HookRunner.RunEvent(sendingArgs, true))
                return false;

            q = sendingArgs.Message;

            var str = $"{player.NetId}!{q}";

            ApiLog.Info("Admin Chat", $"Player &3{player.Name}&r (&6{player.UserId}&r) sent a message: &1{q}&r");
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"[{player.Name}] {q}", ServerLogs.ServerLogType.AdminChat);

            foreach (var ply in ExPlayer.Players)
            {
                if (!ply.HasRemoteAdminAccess || !ply.IsVerified)
                    continue;

                ply.Hub.encryptedChannelManager.TrySendMessageToClient(str, EncryptedChannelManager.EncryptedChannel.AdminChat);
            }

            return false;
        }
    }
}