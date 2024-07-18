using CommandSystem;
using LabExtended.Commands.Formatting;

namespace LabExtended.API.CustomCommands.Formatting
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class FormattingParentCommand : ParentCommand
    {
        public FormattingParentCommand()
            => LoadGeneratedCommands();

        public override string Command => "formatting";
        public override string Description => "Parent command for command formatting help.";

        public override string[] Aliases { get; } = Array.Empty<string>();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new FormattingPlayerListCommand());
            RegisterCommand(new FormattingDurationCommand());
            RegisterCommand(new FormattingEnumCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please use one of these valid subcommands:\n";

            foreach (var cmd in Commands.Values)
                response += $"- formatting {cmd.Command} ({cmd.Description}) | Aliases: {(cmd.Aliases?.Length < 1 ? "none" : string.Join(",", cmd.Aliases))}\n";

            return true;
        }
    }
}