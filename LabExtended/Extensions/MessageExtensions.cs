using LabExtended.API;
using LabExtended.API.Messages;
using LabExtended.API.Interfaces;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting the <see cref="IMessage"/> interface.
/// </summary>
public static class MessageExtensions
{
    /// <summary>
    /// Displays a message.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="target">The player to display the message to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void DisplayMessage(this IMessage message, ExPlayer target)
    {
        if (message is null)
            throw new ArgumentNullException(nameof(message));
        
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (!target)
            return;

        if (message is API.Messages.BroadcastMessage broadcastMessage)
        {
            var flags = Broadcast.BroadcastFlags.Normal;
            
            if (broadcastMessage.IsTruncated)
                flags |= Broadcast.BroadcastFlags.Truncated;
            
            if (broadcastMessage.IsAdminChat)
                flags |= Broadcast.BroadcastFlags.AdminChat;
            
            target.SendBroadcast(broadcastMessage.Content, broadcastMessage.Duration, flags, broadcastMessage.ClearPrevious);
        }
        else if (message is HintMessage hintMessage)
        {
            target.SendHint(hintMessage.Content, hintMessage.Duration);
        }
    }
}