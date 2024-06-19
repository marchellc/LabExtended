namespace LabExtended.API.Messages
{
    public struct HintMessage : IMessage
    {
        public string Content { get; }
        public ushort Duration { get; }

        public HintMessage(string content, ushort duration)
        {
            Content = content;
            Duration = duration;
        }
    }
}