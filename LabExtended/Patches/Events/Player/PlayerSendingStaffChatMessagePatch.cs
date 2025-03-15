using HarmonyLib;

using LabExtended.API;
using LabExtended.API.Enums;

using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Attributes;

using LabExtended.Events;
using LabExtended.Events.Player;

using RemoteAdmin;

namespace LabExtended.Patches.Events.Player
{
    public static class PlayerSendingStaffChatMessagePatch
    {
        [EventPatch(typeof(PlayerSendingStaffChatMessageEventArgs))]
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

            q = Misc.SanitizeRichText(q, "[", "]");

            if (string.IsNullOrWhiteSpace(q.Remove("@")))
                return false;

            if (q.Length >= 2000)
                q = q.SubstringPostfix(2000, "...");

            var sendingArgs = new PlayerSendingStaffChatMessageEventArgs(player, q);

            if (!ExPlayerEvents.OnSendingStaffChatMessage(sendingArgs) && !player.IsNorthwoodStaff)
                return false;

            q = sendingArgs.Message;

            var str = $"{player.NetworkId}!{q}";

            ApiLog.Info("Admin Chat", $"Player &3{player.Nickname}&r (&6{player.UserId}&r) sent a message: &1{q}&r");
            ServerLogs.AddLog(ServerLogs.Modules.Administrative, $"[{player.Nickname}] {q}", ServerLogs.ServerLogType.AdminChat);

            foreach (var ply in ExPlayer.Players)
            {
                if (!ply.HasRemoteAdminAccess || !ply.IsVerified)
                    continue;

                ply.ReferenceHub.encryptedChannelManager.TrySendMessageToClient(str, EncryptedChannelManager.EncryptedChannel.AdminChat);
            }

            return false;
        }
    }
}