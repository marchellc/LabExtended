using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Ticking;
using LabExtended.Extensions;

namespace LabExtended.API.CustomCommands.Tick.Status
{
    public class StatusCommand : CustomCommand
    {
        public override string Command => "status";
        public override string Description => "Shows the status of tick distribution.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (TickDistribution.Distributors.Count < 1)
            {
                ctx.RespondOk("There aren't any tick distributors.");
                return;
            }

            var str = $"Tick Distributors ({TickDistribution.Distributors.Count}):\n";

            TickDistribution.Distributors.ForEach(x =>
            {
                var distributor = x.Item2;

                str += $"[{distributor.GetType().Name}]: {distributor.HandleCount} handle(s), {distributor.TickRate} TPS (MaxTickTime={distributor.MaxTickTime} ms, MinTickTime={distributor.MinTickTime} ms, MaxTickRate={distributor.MaxTickRate} TPS, MinTickRate={distributor.MinTickRate} TPS)\n";
            });

            ctx.RespondOk(str);
        }
    }
}