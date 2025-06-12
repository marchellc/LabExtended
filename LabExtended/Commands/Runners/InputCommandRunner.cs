using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;

using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Runners;

/// <summary>
/// Command runner to use when a command requires more input.
/// </summary>
public class InputCommandRunner : ICommandRunner
{
    private CommandContext context;
    private CommandBase instance;
    
    /// <summary>
    /// The singleton instance of <see cref="InputCommandRunner"/>
    /// </summary>
    public static InputCommandRunner Singleton { get; } = new();

    /// <inheritdoc cref="ICommandRunner.Create"/>
    public ICommandRunner Create(CommandContext context)
    {
        var runner = new InputCommandRunner();

        runner.context = context;
        runner.instance = context.Instance;

        return runner;
    }

    /// <inheritdoc cref="ICommandRunner.ShouldContinue"/>
    public bool ShouldContinue(CommandExecutingEventArgs ev, ExPlayer sender)
    {
        if (sender.activeRunner is not InputCommandRunner inputCommandRunner)
            return false;

        if (inputCommandRunner.context is null || inputCommandRunner.instance is null)
            return false;

        if (inputCommandRunner.context.Response is null)
            return false;

        if (inputCommandRunner.context.Response.onInput is null)
            return false;

        ev.IsAllowed = false;

        var line = string.Join(" ", ev.Arguments.Array);
        var args = ListPool<string>.Shared.Rent(ev.Arguments.Array);
        var response = inputCommandRunner.context.Response;
        
        inputCommandRunner.context.Response = null;
        
        inputCommandRunner.context.Args = args;
        inputCommandRunner.context.Line = line;
        
        inputCommandRunner.context.Type = ev.CommandType;
        
        response.onInput.InvokeSafe(line);

        context.WriteResponse(out _);
        context.InvokeExecuted();
        
        return true;
    }

    /// <inheritdoc cref="ICommandRunner.ShouldPool"/>
    public bool ShouldPool(CommandContext ctx)
        => false;

    /// <inheritdoc cref="ICommandRunner.Run"/>
    public void Run(CommandContext ctx, object[] buffer)
    { }
}