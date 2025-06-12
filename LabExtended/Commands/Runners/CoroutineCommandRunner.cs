using LabApi.Events.Arguments.ServerEvents;

using LabExtended.API;
using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

using MEC;

namespace LabExtended.Commands.Runners;

/// <summary>
/// Used to execute coroutine overloads.
/// </summary>
public class CoroutineCommandRunner : ICommandRunner
{
    private static IEnumerator<float> cachedCoroutine;
    
    private bool isFinished = false;
    private bool isInProgress = false;

    private IEnumerator<float> coroutine;
    
    private Action onFinished;

    /// <summary>
    /// Creates a new <see cref="CoroutineCommandRunner"/> instance.
    /// </summary>

    /// <exception cref="ArgumentNullException"></exception>
    public CoroutineCommandRunner(IEnumerator<float> coroutine, Action onFinished)
    {
        if (coroutine is null)
            throw new ArgumentNullException(nameof(coroutine));
        
        if (onFinished is null)
            throw new ArgumentNullException(nameof(onFinished));
        
        this.coroutine = coroutine;
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

        Timing.RunCoroutine(Helper());
        return;

        IEnumerator<float> Helper()
        {
            while (coroutine.MoveNext())
                yield return coroutine.Current;

            isFinished = true;
            isInProgress = false;
            
            onFinished.InvokeSafe();
        }
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