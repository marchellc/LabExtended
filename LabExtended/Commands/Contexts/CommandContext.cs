using LabExtended.API;

using LabExtended.Commands.Arguments;
using LabExtended.Commands.Responses;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Responses;

namespace LabExtended.Commands.Contexts
{
    public class CommandContext : ICommandContext
    {
        public string RawInput { get; }
        public string[] RawArgs { get; }

        public ArgumentCollection Args { get; }
        public ExPlayer Sender { get; }

        public bool IsHost { get; }

        public ICommandResponse Response { get; private set; }

        public CommandContext(string arg, string[] args, ArgumentCollection collection, ExPlayer sender)
        {
            RawInput = arg;
            RawArgs = args;

            Args = collection;

            Sender = sender;
            IsHost = sender.IsServer;
        }

        public void Respond(object response, bool success)
        {
            if (Response != null)
                throw new InvalidOperationException($"A response has already been created.");

            if (response is null || response is "" || response is " ")
                throw new ArgumentNullException(nameof(response));

            Response = new CommandResponse(response.ToString(), success);
        }

        public void Respond(IEnumerable<object> lines, bool success)
        {
            if (Response != null)
                throw new InvalidOperationException($"A response has already been created.");

            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            var str = "";

            foreach (var line in lines)
                str += $"{line}\n";

            Response = new CommandResponse(str, success);
        }

        public void RespondFail(object response)
            => Respond(response, false);

        public void RespondFail(IEnumerable<object> lines)
            => Respond(lines, false);

        public void RespondOk(object response)
            => Respond(response, true);

        public void RespondOk(IEnumerable<object> lines)
            => Respond(lines, true);

        public void RespondContinued(object response, Action<ContinuedContext> onContinued)
        {
            if (Response != null)
                throw new InvalidOperationException($"A response has already been created.");

            if (response is null || response is "" || response is " ")
                throw new ArgumentNullException(nameof(response));

            if (onContinued is null)
                throw new ArgumentNullException(nameof(onContinued));

            Response = new ContinuedResponse(response.ToString(), onContinued);
        }

        public void RespondContinued(IEnumerable<object> lines, Action<ContinuedContext> onContinued)
        {
            if (Response != null)
                throw new InvalidOperationException($"A response has already been created.");

            if (onContinued is null)
                throw new ArgumentNullException(nameof(onContinued));

            if (lines is null)
                throw new ArgumentNullException(nameof(lines));

            var str = "";

            foreach (var line in lines)
                str += $"{line}\n";

            Response = new ContinuedResponse(str, onContinued);
        }
    }
}