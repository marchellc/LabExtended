// #define ENABLE_TEST_COMMANDS

#if ENABLE_TEST_COMMANDS
using LabExtended.Commands;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using MEC;

namespace LabExtended.Utilities.Testing.Commands;

[Command("coroutinetest", "Tests the coroutine commands.")]
public class CoroutineCommandTest : CommandBase, IServerSideCommand
{
    [CommandOverload]
    public IEnumerator<float> Invoke(
        [CommandParameter("Wait Time", "How long to wait (in seconds)")] int seconds = 5)
    {
        Write($"OK! Waiting {seconds} second(s) ..");

        yield return Timing.WaitForSeconds(seconds);

        Ok($"Waited {seconds} second(s)!");
    }
}
#endif