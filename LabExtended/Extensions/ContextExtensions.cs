using System.Text;

using LabExtended.Commands.Interfaces;

using NorthwoodLib.Pools;

namespace LabExtended.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ICommandContext"/>.
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Responds to a context with a StringBuilder.
        /// </summary>
        /// <param name="ctx">The context to respond to.</param>
        /// <param name="builder">The delegate used to build the command's response.</param>
        public static void RespondOk(this ICommandContext ctx, Action<StringBuilder> builder)
            => Respond(ctx, true, builder);
        
        /// <summary>
        /// Responds to a context with a StringBuilder.
        /// </summary>
        /// <param name="ctx">The context to respond to.</param>
        /// <param name="builder">The delegate used to build the command's response.</param>
        public static void RespondFail(this ICommandContext ctx, Action<StringBuilder> builder)
            => Respond(ctx, false, builder);
        
        /// <summary>
        /// Responds to a context with a StringBuilder.
        /// </summary>
        /// <param name="ctx">The context to respond to.</param>
        /// <param name="status">Whether or not the command's execution was a success.</param>
        /// <param name="builder">The delegate used to build the command's response.</param>
        public static void Respond(this ICommandContext ctx, bool status, Action<StringBuilder> builder)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));
            
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            
            ctx.Respond(StringBuilderPool.Shared.BuildString(builder), status);
        }
        
        /// <summary>
        /// Sends a message into the sender's console.
        /// </summary>
        /// <param name="ctx">The command context.</param>
        /// <param name="message">The message to show.</param>
        /// <param name="showAsSuccess">Whether or not to show this message as a successful response.</param>
        public static void Message(this ICommandContext ctx, object message, bool showAsSuccess = true)
        {
            if (ctx is null)
                throw new ArgumentNullException(nameof(ctx));
            
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            ctx.Sender.SendRemoteAdminMessage(message, showAsSuccess, true, ctx.Command.Command);
        }
    }
}