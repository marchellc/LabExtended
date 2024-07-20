using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Commands.Responses;

namespace LabExtended.Commands.Contexts
{
    public class ContinuedContext : CommandContext
    {
        public ContinuedContext(ContinuedResponse prevResponse, ICommandContext ctx, string arg, string[] args) : base(arg, args, ctx.Args, ctx.Command, ctx.Sender)
        {
            PreviousContext = ctx;
            PreviousResponse = prevResponse;
        }

        public ICommandContext PreviousContext { get; internal set; }
        public ContinuedResponse PreviousResponse { get; internal set; }
    }
}