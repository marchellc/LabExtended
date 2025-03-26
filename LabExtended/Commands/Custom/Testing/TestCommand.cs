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
    [CommandOverload]
    public void TestOverload(
        [CommandParameter("Word", "Testing word")] string word,
        [CommandParameter("Second Word", "The second testing word")] string secondWord,
        [CommandParameter("Third Word", "The third testing word")] string thirdWord)
    {
        Ok($"Test passed: {word} + {secondWord} + {thirdWord}");
    }

    [CommandOverload("list", "Tests list behaviour")]
    public void ListOverload([CommandParameter("List", "The list of words")] List<string> words)
    {
        Ok($"Words ({words.Count}): {string.Join(" + ", words)}");
    }

    [CommandOverload("dict", "Tests dictionary behaviour")]
    public void DictionaryOverload(
        [CommandParameter("Dictionary", "The dictionary of words")] Dictionary<string, string> words)
    {
        Ok($"Words ({words.Count}): {string.Join(" + ", words)}");
    }

    /// <inheritdoc cref="CommandBase.OnInitializeOverload"/>
    public override void OnInitializeOverload(string? overloadName, Dictionary<string, CommandParameterBuilder> parameters)
    {
        base.OnInitializeOverload(overloadName, parameters);

        if (overloadName is null)
        {
            parameters["word"]
                .WithRestriction(new StringLengthRangeRestriction(1, 3));
        }
    }
}