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
    /// A default no-body testing overload.
    /// </summary>
    [CommandOverload("test")]
    public void TestOverload(string word)
    {
        Ok($"Test passed: {word}");
    }

    /// <inheritdoc cref="CommandBase.OnInitializeOverload"/>
    public override void OnInitializeOverload(string overloadName, Dictionary<string, CommandParameterBuilder> parameters)
    {
        base.OnInitializeOverload(overloadName, parameters);

        parameters["word"].WithDescription("A word");
    }
}