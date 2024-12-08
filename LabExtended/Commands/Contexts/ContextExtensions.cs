using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Contexts
{
    public static class ContextExtensions
    {
        public static void Message(this ICommandContext ctx, object message, bool showAsSuccess = true)
        {
            if (ctx is null || ctx.Sender is null || message is null)
                return;

            ctx.Sender.SendRemoteAdminMessage(message, showAsSuccess, true, ctx.Command.Command);
        }
    }
}