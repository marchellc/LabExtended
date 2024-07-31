namespace LabExtended.Core.Commands.Interfaces
{
    public interface ICommandResponse
    {
        string Response { get; }

        bool IsSuccess { get; }
    }
}