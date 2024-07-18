using LabExtended.API;
using LabExtended.Commands.Arguments;
using LabExtended.Core.Commands.Responses;

namespace LabExtended.Core.Commands.Interfaces
{
    public interface ICommandContext
    {
        string RawInput { get; }
        string[] RawArgs { get; }

        ArgumentCollection Args { get; }

        ExPlayer Sender { get; }

        bool IsHost { get; }

        void Respond(object response, bool success);

        void RespondOk(object response);
        void RespondFail(object response);

        void RespondContinued(object message, Action<ContinuedContext> onContinued);
    }
}