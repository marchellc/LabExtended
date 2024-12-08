using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Core.Ticking;
using LabExtended.Extensions;
using NorthwoodLib.Pools;

namespace LabExtended.API.CustomCommands.Tick.List
{
    public class ListCommand : CustomCommand
    {
        public override string Command => "list";
        public override string Description => "Lists all handles in a specific distributor.";

        public override ArgumentDefinition[] BuildArgs()
            => GetOptionalArg("Type", "Tick distributor name.", string.Empty);

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            if (TickDistribution.Distributors.Count < 1)
            {
                ctx.RespondOk("There aren't any active distributors.");
                return;
            }

            var name = args.Get<string>("Type");

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (!TickDistribution.Distributors.TryGetFirst(x => x.GetType().Name.ToLower().Contains(name.ToLower()), out var targetDistributor))
                {
                    ctx.RespondFail("Unknown distributor name.");
                    return;
                }

                var builder = StringBuilderPool.Shared.Rent();
                var handles = targetDistributor.Item2.Handles.OrderBy(x => x);

                builder.AppendLine($"Displaying all handles in '{targetDistributor.GetType().Name}' ({targetDistributor.Item2.HandleCount}):");

                foreach (var handle in handles)
                    builder.Append("- ").Append(handle).AppendLine();

                ctx.RespondOk(StringBuilderPool.Shared.ToStringReturn(builder));
            }
            else
            {
                var builder = StringBuilderPool.Shared.Rent();

                TickDistribution.Distributors.ForEach(t => 
                {
                    var distributor = t.Item2;
                    var handles = distributor.Handles.OrderBy(x => x);

                    builder.AppendLine($"Displaying all handles in '{distributor.GetType().Name}' ({distributor.HandleCount}):");

                    foreach (var handle in handles)
                        builder.Append("- ").Append(handle).AppendLine();

                    builder.AppendLine();
                });

                ctx.RespondOk(StringBuilderPool.Shared.ToStringReturn(builder));
            }
        }
    }
}