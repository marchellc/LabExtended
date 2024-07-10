using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Responses
{
    public class ContinuedResponse : ICommandResponse
    {
        public string Response { get; }

        public bool IsSuccess => true;

        public ContinuedResponse(string response)
            => Response = response;
    }
}