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
        public override string Command => "performance";
        public override string Description => "Shows your server's performance stats.";

        public override string[] Aliases { get; } = new string[] { "perf" };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

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
            builder.AppendLine($"- Memory Usage: {(global::Mirror.Utils.PrettyBytes(smemcur))} (Max Used: {(global::Mirror.Utils.PrettyBytes(smemmax))})");

            ctx.RespondOk(StringBuilderPool.Shared.ToStringReturn(builder));
        }
    }
}