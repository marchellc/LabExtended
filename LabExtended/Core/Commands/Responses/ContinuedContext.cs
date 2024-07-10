using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Core.Commands.Responses
{
    public class ContinuedContext : ICommandContext
    {
        public ICommandContext PreviousContext { get; }
        public ICommandResponse PreviousResponse { get; }

        internal ContinuedContext(ICommandContext context, ICommandResponse response)
        {
            PreviousContext = context;
            PreviousResponse = response;
        }
    }
}