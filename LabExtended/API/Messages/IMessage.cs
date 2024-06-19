namespace LabExtended.API.Messages
{
    public interface IMessage
    {
        string Content { get; }

        ushort Duration { get; }
    }
}