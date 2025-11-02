using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Utilities;

namespace LabExtended.Commands.Custom.Performance;

/// <summary>
/// Provides a command for displaying detailed performance statistics and running the profiler for a specified duration.
/// </summary>
[Command("performance", "Shows detailed performance statistics.", "perf")]
public partial class PerformanceCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Runs the profiler for the specified duration in milliseconds.
    /// </summary>
    /// <param name="duration">The length of time, in milliseconds, to run the profiler. Must be a non-negative value.</param>
    [CommandOverload(null, null)]
    public void InvokeCommand(int duration)
    {
        ProfilerUtils.Run(duration);
        
        Ok($"Running the profiler for '{ProfilerUtils.Duration}' milliseconds ({ProfilerUtils.Duration - ProfilerUtils.Time.ElapsedMilliseconds} remaining)");
    }
}