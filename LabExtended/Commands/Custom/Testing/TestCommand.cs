using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;
using LabExtended.Commands.Parameters.Arguments;

namespace LabExtended.Commands.Custom.Testing;

/// <summary>
/// A command used for testing the new framework.
/// </summary>
[Command("test", "Tests the new command framework.")]
public class TestCommand : CommandBase, IAllCommand
{
    /// <summary>
    /// A default testing overload.
    /// </summary>
    [CommandOverload("test")]
    public void TestOverload(
        [CommandParameter("Word", "Testing word")] string word,
        [CommandParameter("Second Word", "The second testing word")] string secondWord,
        [CommandParameter("Third Word", "The third testing word")] string thirdWord)
    {
        Ok($"Test passed: {word} + {secondWord} + {thirdWord}");
    }

    /// <inheritdoc cref="CommandBase.OnInitializeOverload"/>
    public override void OnInitializeOverload(Dictionary<string, CommandParameterBuilder> parameters)
    {
        base.OnInitializeOverload(parameters);

        parameters["word"]
            .WithArgument(new StringLengthRangeArgument(1, 3));
    }
}