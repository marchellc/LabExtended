using LabExtended.API;
using LabExtended.Core.Events;

namespace LabExtended.Events.Player
{
    public class PlayerSendingAdminChatMessageArgs : BoolCancellableEvent
    {
        public ExPlayer Player { get; }

        public string Message { get; set; }

        internal PlayerSendingAdminChatMessageArgs(ExPlayer player, string msg)
        {
            Player = player;
            Message = msg;
        }
    }
}