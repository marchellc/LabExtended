using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

namespace LabExtended.Commands.Custom.Performance;

[Command("performance", "Shows detailed performance statistics.", "perf")]
public partial class PerformanceCommand : CommandBase, IServerSideCommand
{
    [CommandOverload]
    public void InvokeCommand(int duration)
    {
        ProfilerUtils.Run(duration);
        
        Ok($"Running the profiler for '{ProfilerUtils.Duration}' milliseconds ({ProfilerUtils.Duration - ProfilerUtils.Time.ElapsedMilliseconds} remaining)");
    }
}