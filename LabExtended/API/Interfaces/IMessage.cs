namespace LabExtended.API.Interfaces
{
    public interface IMessage
    {
        string Content { get; set; }

        ushort Duration { get; set; }
    }
}