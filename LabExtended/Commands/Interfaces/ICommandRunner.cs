using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;

namespace LabExtended.Commands.Interfaces;

/// <summary>
/// A base interface for command runners.
/// </summary>
public interface ICommandRunner
{
    /// <summary>
    /// Gets a runner instance for a specific context.
    /// </summary>
    /// <param name="context">The target context.</param>
    /// <returns>The runner instance.</returns>
    ICommandRunner Create(CommandContext context);

    /// <summary>
    /// Whether or not a previously assigned runner instance should be continued on this input.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    /// <param name="sender">The player who sent the input.</param>
    /// <returns></returns>
    bool ShouldContinue(CommandExecutingEventArgs args, ExPlayer sender);

    /// <summary>
    /// Whether or not a command instance from a context should be pooled.
    /// </summary>
    /// <param name="ctx">The target context.</param>
    /// <returns>true if the instance should be pooled</returns>
    bool ShouldPool(CommandContext ctx);
    
    /// <summary>
    /// Registers a player input.
    /// </summary>
    /// <param name="ctx">The command context.</param>
    /// <param name="buffer">The command overload buffer.</param>
    void Run(CommandContext ctx, object[] buffer);
}