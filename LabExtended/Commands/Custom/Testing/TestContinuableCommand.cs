using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using MEC;

namespace LabExtended.Commands.Custom.Testing;

/// <summary>
/// Tests continuable commands.
/// </summary>
[Command("testcont", "Tests continuable commands", TimeOut = 10f)]
public class TestContinuableCommand : ContinuableCommandBase, IAllCommand
{
    public int Loops { get; set; } = 0;

    [CommandOverload]
    public IEnumerator<float> Overload()
    {
        Write("Waiting for 5 secs");

        yield return Timing.WaitForSeconds(5f);
        
        Continue("Wait finished");
    }
    
    /// <inheritdoc cref="ContinuableCommandBase.OnContinued"/>
    public override void OnContinued()
    {
        Loops++;
        
        Continue($"Loops: {Loops}");
    }

    /// <inheritdoc cref="ContinuableCommandBase.OnTimedOut"/>
    public override void OnTimedOut()
    {
        Write($"Timed out", false);
    }
}