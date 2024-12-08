using LabExtended.API.Interfaces;

namespace LabExtended.API.Messages
{
    public class HintMessage : IMessage
    {
        public string Content { get; set; }
        public ushort Duration { get; set; }

        public HintMessage(string content, ushort duration)
        {
            Content = content;
            Duration = duration;
        }
    }
}