// #define ENABLE_TEST_COMMANDS

#if ENABLE_TEST_COMMANDS
using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Utilities.Testing.Commands;

[Command("asynctest", "Tests asynchronous command overloads.")]
public class AsyncCommandTest : CommandBase, IServerSideCommand
{
    [CommandOverload]
    public async Task InvokeAsync(
        [CommandParameter("Delay", "How many seconds to wait.")] int delay = 5)
    {
        WriteThread($"Waiting for {delay} second(s) ..");
        
        await Task.Delay(delay * 1000);
        
        Ok($"Waited {delay} second(s)!");
    }
}
#endif