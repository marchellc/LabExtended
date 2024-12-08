namespace LabExtended.Commands.Contexts
{
    public class ContinuedContext : CommandContext
    {
        public ContinuedContext(Responses.ContinuedResponse prevResponse, Interfaces.ICommandContext ctx, string arg, string[] args) : base(arg, args, ctx.Args, ctx.Command, ctx.Sender)
        {
            PreviousContext = ctx;
            PreviousResponse = prevResponse;
        }

        public Interfaces.ICommandContext PreviousContext { get; internal set; }
        public Responses.ContinuedResponse PreviousResponse { get; internal set; }
    }
}