using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Ticking;

namespace LabExtended.API.CustomCommands.Tick.Status
{
    public class TickStatusCommand : CustomCommand
    {
        public override string Command => "status";
        public override string Description => "Shows the status of tick distribution.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (TickDistribution.AllDistributors.Count() < 1)
            {
                ctx.RespondOk("There aren't any tick distributors.");
                return;
            }

            var str = $"Tick Distributors ({TickDistribution.AllDistributors.Count()}):\n";

            foreach (var distributor in TickDistribution.AllDistributors)
                str += $"[{distributor.GetType().Name}]: {distributor.HandleCount} handle(s), {distributor.TickRate} TPS\n";

            ctx.RespondOk(str);
        }
    }
}