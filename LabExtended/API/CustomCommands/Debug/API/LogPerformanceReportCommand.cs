using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;
using LabExtended.Core.Performance;
using LabExtended.Core.Profiling;

namespace LabExtended.API.CustomCommands.Debug.API
{
    public class LogPerformanceReportCommand : CustomCommand
    {
        public override string Command => "logperformance";
        public override string Description => "Logs all profiler markers and performance statistics to the server console.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            ProfilerMarker.LogAllMarkers(false);
            PerformanceWatcher.CurReport.LogToConsole();

            ctx.RespondOk("Logged performance statistics to the server console.");
        }
    }
}