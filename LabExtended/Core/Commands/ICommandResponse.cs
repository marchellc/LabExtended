namespace LabExtended.Core.Commands
{
    public interface ICommandResponse
    {
        string Text { get; }

        bool IsSuccess { get; }
    }
}