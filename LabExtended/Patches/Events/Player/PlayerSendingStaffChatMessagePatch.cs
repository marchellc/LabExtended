using HarmonyLib;

using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.Handlers;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Events;

using RemoteAdmin;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerSendingStaffChatMessagePatch
    {
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

            if (!ExPlayerEvents.OnReceivingRemoteAdminRequest(new(player, RemoteAdminRequestType.StaffChat, q))
                && !player.IsNorthwoodStaff)
                return false;

            q = Misc.SanitizeRichText(q.Replace("~", "-"), "[", "]").Trim();

            if (string.IsNullOrWhiteSpace(q.Remove("@")))
                return false;

            if (q.Length >= 2000)
                q = q.SubstringPostfix(2000, "...");

            var sendingAdminChatMessageEventArgs = new SendingAdminChatEventArgs(sender, q);
            
            ServerEvents.OnSendingAdminChat(sendingAdminChatMessageEventArgs);

            if (!sendingAdminChatMessageEventArgs.IsAllowed)
            {
                if (sender is PlayerCommandSender playerCommandSender)
                {
                    playerCommandSender.ReferenceHub.gameConsoleTransmission.SendToClient("A server plugin cancelled the message.", "red");
                    playerCommandSender.RaReply("A server plugin cancelled the message.", success: false, logToConsole: true, "");
                }

                return false;
            }

            q = sendingAdminChatMessageEventArgs.Message;

            var str = $"{player.NetworkId}!{q}";

            ApiLog.Info("Admin Chat", $"Player &3{player.Nickname}&r (&6{player.UserId}&r) sent a message: &1{q}&r");
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"[{player.Nickname}] {q}", ServerLogs.ServerLogType.AdminChat);

            foreach (var ply in ExPlayer.Players)
            {
                if (!ply.IsVerified || !ply.HasStaffChatAccess)
                    continue;

                ply.ReferenceHub.encryptedChannelManager.TrySendMessageToClient(str, EncryptedChannelManager.EncryptedChannel.AdminChat);
            }

            ServerEvents.OnSentAdminChat(new(sender, q));
            return false;
        }
    }
}