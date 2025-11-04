using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.Commands.Interfaces;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Utilities;

namespace LabExtended.Commands.Runners;

/// <summary>
/// Used to execute async overloads.
/// </summary>
public class AsyncCommandRunner : ICommandRunner
{
    private bool isFinished = false;
    private bool isInProgress = false;

    private Task task;
    private Action onFinished;

    /// <summary>
    /// Creates a new <see cref="AsyncCommandRunner"/> instance.
    /// </summary>
    public AsyncCommandRunner(Task task, Action onFinished)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));
        
        if (onFinished is null)
            throw new ArgumentNullException(nameof(onFinished));
        
        this.task = task;
        this.onFinished = onFinished;
    }

    /// <summary>
    /// Starts the coroutine.
    /// </summary>
    public void Start()
    {
        if (isFinished || isInProgress)
            return;

        isFinished = false;
        isInProgress = true;

        Task.Run(async () =>
        {
            await task;
        }).ContinueWithOnMain(x =>
        {
            if (x.Exception != null)
                ApiLog.Error("CommandManager", x.Exception);
            
            isFinished = true;
            isInProgress = false;

            onFinished?.InvokeSafe();
        });
    }

    /// <inheritdoc cref="ICommandRunner.Create"/>
    public ICommandRunner Create(CommandContext context)
        => this;

    /// <inheritdoc cref="ICommandRunner.ShouldContinue"/>
    public bool ShouldContinue(CommandExecutingEventArgs args, ExPlayer sender)
        => isFinished;

    /// <inheritdoc cref="ICommandRunner.ShouldPool"/>
    public bool ShouldPool(CommandContext ctx)
        => false;

    /// <inheritdoc cref="ICommandRunner.Run"/>
    public void Run(CommandContext ctx, object[] buffer) 
    { }
}