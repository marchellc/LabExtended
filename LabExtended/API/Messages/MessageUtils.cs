namespace LabExtended.API.Messages
{
    public static class MessageUtils
    {
        public static bool IsValid(this IMessage message)
            => message != null && message.Duration > 0 && !string.IsNullOrWhiteSpace(message.Content);

        public static void Show(this IMessage message, IEnumerable<ExPlayer> args)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            if (!message.IsValid())
                return;

            foreach (var player in args)
            {
                if (message is HintMessage)
                    player.Hint(message.Content, message.Duration);
                else if (message is BroadcastMessage broadcastMessage)
                    player.Broadcast(broadcastMessage.Content, broadcastMessage.Duration, broadcastMessage.ClearPrevious);
                else
                    throw new InvalidOperationException($"Unknown IMessage type: {message.GetType().FullName}");
            }
        }

        public static void ShowGlobal(this IMessage message)
            => message.Show(ExPlayer.Players);
    }
}