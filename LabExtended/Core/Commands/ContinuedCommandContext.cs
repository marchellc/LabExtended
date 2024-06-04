using LabExtended.API;

namespace LabExtended.Core.Commands
{
    public class ContinuedCommandContext : CommandContext
    {
        public bool IsTimedOut { get; }

        internal ContinuedCommandContext(CommandModule module, CommandInfo command, ExPlayer sender, string line, string[] args, bool isTimedOut) : base(module, command, sender, line, args)
            => IsTimedOut = isTimedOut;
    }
}