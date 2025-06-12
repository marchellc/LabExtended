using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;

using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

namespace LabExtended.Commands.Runners;

/// <summary>
/// Runner for commands that return void.
/// </summary>
public class RegularCommandRunner : ICommandRunner
{
    private RegularCommandRunner() { }
    
    /// <summary>
    /// Gets the singleton instance of <see cref="RegularCommandRunner"/>.
    /// </summary>
    public static RegularCommandRunner Singleton { get; } = new();

    /// <inheritdoc cref="ICommandRunner.Create"/>
    public ICommandRunner Create(CommandContext context)
        => this;
    
    /// <inheritdoc cref="ICommandRunner.ShouldPool"/>
    public bool ShouldPool(CommandContext ctx)
        => true;

    /// <inheritdoc cref="ICommandRunner.ShouldContinue"/>
    public bool ShouldContinue(CommandExecutingEventArgs args, ExPlayer sender)
        => false;

    /// <inheritdoc cref="ICommandRunner.Run"/>
    public void Run(CommandContext ctx, object[] buffer)
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        var prevRunner = ctx.Sender.activeRunner;

        void FinishCommand()
        {
            if (ctx.Sender.activeRunner is CoroutineCommandRunner or AsyncCommandRunner)
                ctx.Sender.activeRunner = prevRunner;

            ctx.WriteResponse(out _);
            ctx.InvokeExecuted();
        }

        try
        {
            var result = ctx.Overload.Method(ctx.Instance, buffer);

            if (result is IEnumerator<float> coroutine)
            {
                var runner = new CoroutineCommandRunner(coroutine, FinishCommand);

                prevRunner = ctx.Sender.activeRunner;

                ctx.Sender.activeRunner = runner;
                
                runner.Start();
                return;
            }
            
            if (result is Task task)
            {
                var runner = new AsyncCommandRunner(task, FinishCommand);
                
                prevRunner = ctx.Sender.activeRunner;
                
                ctx.Sender.activeRunner = runner;
                
                runner.Start();
                return;
            }
        }
        catch (Exception ex)
        {
            ctx.Response = new(false, false, false, null, ctx.FormatExceptionResponse(ex));
        }
        
        FinishCommand();
    }
}