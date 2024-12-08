using LabExtended.Commands.Contexts;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Responses
{
    public class ContinuedResponse : ICommandResponse
    {
        internal Action<ContinuedContext> _onContinued;

        public string Response { get; }

        public bool IsSuccess => true;

        public ContinuedResponse(string response, Action<ContinuedContext> onContinued)
            => (Response, _onContinued) = (response, onContinued);
    }
}