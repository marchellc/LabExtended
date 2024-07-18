using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Commands.Responses
{
    public struct CommandResponse : ICommandResponse
    {
        public string Response { get; }
        public bool IsSuccess { get; }

        public CommandResponse(string response, bool success)
        {
            Response = response;
            IsSuccess = success;
        }
    }
}