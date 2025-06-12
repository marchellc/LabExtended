#define ENABLE_TEST_COMMANDS

#if ENABLE_TEST_COMMANDS
using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Utilities.Testing.Commands;

[Command("inputtest", "Tests the input response.")]
public class InputCommandTest : CommandBase, IServerSideCommand
{
    [CommandOverload]
    public void Invoke()
    {
        Read("Type something.", result =>
        {
            Read("Type another thing.", resultTwo =>
            {
                Ok($"First result: {result}; Second result: {resultTwo}");
            });
        });
    }

    [CommandOverload("integer", "Tests an integer input parsing.")]
    public void InvokeInt()
    {
        Read<int>("Type a number.", num => Ok($"Typed number: {num}"));
    }
}
#endif