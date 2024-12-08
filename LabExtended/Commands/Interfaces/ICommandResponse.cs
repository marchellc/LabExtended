namespace LabExtended.Commands.Interfaces
{
    public interface ICommandResponse
    {
        string Response { get; }

        bool IsSuccess { get; }
    }
}