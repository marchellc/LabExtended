using LabExtended.API.Interfaces;

namespace LabExtended.API.Messages
{
    public class BroadcastMessage : IMessage
    {
        public string Content { get; set; }

        public ushort Duration { get; set; }

        public bool ClearPrevious { get; set; }

        public bool IsTruncated { get; set; }
        public bool IsAdminChat { get; set; }

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