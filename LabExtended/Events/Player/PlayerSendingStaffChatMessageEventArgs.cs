using LabExtended.API;

namespace LabExtended.Events.Player
{
    /// <summary>
    /// Gets called when a player sends a message into the Staff Chat.
    /// </summary>
    public class PlayerSendingStaffChatMessageEventArgs : BooleanEventArgs
    {
        /// <summary>
        /// The player who is sending a message.
        /// </summary>
        public ExPlayer Player { get; }

        /// <summary>
        /// The message to be sent.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Creates a new <see cref="PlayerSendingStaffChatMessageEventArgs"/> instance.
        /// </summary>
        /// <param name="player">The player sending the message.</param>
        /// <param name="msg">The message.</param>
        public PlayerSendingStaffChatMessageEventArgs(ExPlayer player, string msg)
        {
            Player = player;
            Message = msg;
        }
    }
}