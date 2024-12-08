using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

namespace LabExtended.API.CustomCommands.Formatting
{
    public class FormattingDurationCommand : CustomCommand
    {
        public override string Command => "duration";
        public override string Description => "Describes duration formatting.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            ctx.RespondOk(new string[]
            {
                "Formatting duration is simple.",
                "The general format is xY (where 'x' is the amount of time and 'Y' is the time unit)",
                "For example, '5m' would be 5 minutes. These can be combined endlessly, so '5m 30s' would be 5 minutes and 3 seconds, as well as '5h 10m 30s' would be 5 hours, 10 minutes and 30 seconds.",
                "All time units:",
                " >- s / S(seconds)",
                " >- m (minutes)",
                " -> h / H (hours)",
                " -> d / D (days)",
                " -> M (months)",
                " -> y / Y (years)"
            });
        }
    }
}