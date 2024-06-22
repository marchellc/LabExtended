using LabExtended.API;
using LabExtended.Core.Commands;

namespace LabExtended.Commands.Formatting
{
    public class FormattingDurationCommand : CommandInfo
    {
        public override string Command => "duration";
        public override string Description => "Describes duration formatting.";

        public object OnCalled(ExPlayer player)
            => new string[]
            {
                "Formatting duration is simple.",
                "The general format is xY (where 'x' is the amount of time and 'Y' is the time unit)",
                "For example, '5m' would be 5 minutes. These can be combined endlessly, so '5m 30s' would be 5 minutes and 3 seconds, aswell as '5h 10m 30s' would be 5 hours, 10 minutes and 30 seconds.",
                "All time units:",
                " >- s / S(seconds)",
                " >- m (minutes)",
                " -> h / H (hours)",
                " -> d / D (days)",
                " -> M (months)",
                " -> y / Y (years)"
            };
    }
}