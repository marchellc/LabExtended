using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;

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
        [CommandParameter("Second Word", "The second testing word")] string secondWord)
    {
        Ok($"Test passed: {word} + {secondWord}");
    }
}