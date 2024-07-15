using LabExtended.API.Interfaces;

namespace LabExtended.API.Messages
{
    public struct BroadcastMessage : IMessage
    {
        public string Content { get; }

        public ushort Duration { get; }

        public bool ClearPrevious { get; }

        public bool IsTruncated { get; }
        public bool IsAdminChat { get; }

        public BroadcastMessage(string content, ushort duration, bool clearPrevious, bool isTruncated, bool isAdminChat)
        {
            Content = content;
            Duration = duration;
            ClearPrevious = clearPrevious;
            IsTruncated = isTruncated;
            IsAdminChat = isAdminChat;
        }
    }
}