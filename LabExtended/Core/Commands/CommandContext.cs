using LabExtended.API;
using LabExtended.Core.Commands.Responses;

namespace LabExtended.Core.Commands
{
    public class CommandContext
    {
        public ICommandResponse Response { get; private set; }

        public CommandModule Module { get; }
        public CommandInfo Command { get; }

        public ExPlayer Sender { get; }

        public bool IsServer => Sender.IsServer;
        public bool IsPlayer => !Sender.IsServer;

        public string Line { get; }
        public string[] Args { get; }

        internal CommandContext(CommandModule module, CommandInfo command, ExPlayer sender, string line, string[] args)
        {
            Module = module;
            Command = command;
            Sender = sender;
            Line = line;
            Args = args;
        }

        public void Respond(object text, bool success = true)
        {
            if (Response != null)
                throw new InvalidOperationException($"This command has already been responded to.");

            var str = text?.ToString() ?? null;

            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(text));

            if (success)
                Response = new SuccessResponse(str);
            else
                Response = new ErrorResponse(str, null);
        }

        public void Ok(object response)
            => Respond(response, true);

        public void Fail(object response)
            => Respond(response, false);

        public void Continue(Action<ContinuedCommandContext> callback, object response = null, bool success = true, float timeout = 0f)
        {
            if (Response != null)
                throw new InvalidOperationException($"This command has already been responded to.");

            var str = response?.ToString() ?? null;

            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(response));

            Response = new ContinuedResponse(str, success, timeout, callback);
        }
    }
}