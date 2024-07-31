namespace LabExtended.API.Interfaces
{
    public interface IMessage
    {
        string Content { get; }

        ushort Duration { get; }
    }
}