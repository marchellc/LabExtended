#define ENABLE_PROFILER
using CommandSystem;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using NorthwoodLib.Pools;

using System.Diagnostics;

using LabExtended.Core;

using Unity.Profiling;

using UnityEngine;
using UnityEngine.Profiling;

namespace LabExtended.API.CustomCommands.Performance
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class PerformanceCommand : CustomCommand
    {
        public static Dictionary<string, Recorder> recorders = new Dictionary<string, Recorder>();
        
        public override string Command => "performance";
        public override string Description => "Shows your server's performance stats.";

        public override string[] Aliases { get; } = new string[] { "perf" };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (recorders.Count == 0)
            {
                ApiLog.Debug("Performance Command", $"Setting up profiler recorders");
                
                var categories = new ProfilerCategory[Profiler.GetCategoriesCount()];
                
                ApiLog.Debug("Performance Command", $"Category array: {categories.Length}");
                
                Profiler.GetAllCategories(categories);
                
                ApiLog.Debug("Performance Command", $"Retrieved categories");

                for (int i = 0; i < categories.Length; i++)
                {
                    var category = categories[i];
                    
                    ApiLog.Debug("Performance Command", $"Category: {category.Name}");

                    var recorder = Recorder.Get(category.Name);

                    if (recorder is null)
                    {
                        ApiLog.Debug("Performance Command", $"Recorder for category {category.Name} not found");
                        continue;
                    }
                    
                    recorder.enabled = true;
                    recorders.Add(category.Name, recorder);
                }
            }

            var builder = StringBuilderPool.Shared.Rent();
            var proc = Process.GetCurrentProcess();

            var stps = ExServer.Tps;
            var sftt = ExServer.TargetFrameRate;

            var stim = Time.deltaTime;

            var smemcur = proc.WorkingSet64;
            var smemmax = proc.PeakWorkingSet64;

            builder.AppendLine();
            builder.AppendLine($"- TPS (Ticks per Second): {stps} TPS");
            builder.AppendLine($"- Frame Time (how long it takes to execute a single frame): {stim * 1000} ms");
            builder.AppendLine($"- Targeted Frame Rate (maximum amount of frames per second): {sftt} TPS");
            builder.AppendLine($"- Memory Usage: {(global::Mirror.Utils.PrettyBytes(smemcur))} (Max Used: {(global::Mirror.Utils.PrettyBytes(proc.PeakWorkingSet64))})");

            builder.AppendLine($"- Unity Profilers ({recorders.Count}):");

            foreach (var recorder in recorders)
            {
                if (!recorder.Value.enabled)
                    continue;
                
                recorder.Value.CollectFromAllThreads();
                
                builder.AppendLine($"   - {recorder.Key}: {recorder.Value.elapsedNanoseconds / 1000000} ms ({recorder.Value.sampleBlockCount})");
            }

            ctx.RespondOk(StringBuilderPool.Shared.ToStringReturn(builder));
        }
    }
}