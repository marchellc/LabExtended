using CommandSystem;

namespace LabExtended.Commands.Plugins
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class PluginParentCommand : ParentCommand
    {
        public PluginParentCommand()
            => LoadGeneratedCommands();

        public override string Command => "plugin";
        public override string Description => "Parent for plugin management commands.";

        public override string[] Aliases { get; } = Array.Empty<string>();

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new PluginReloadCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = "Please use one of these valid subcommands:\n";

            foreach (var cmd in Commands.Values)
                response += $"- plugin {cmd.Command} ({cmd.Description}) | Aliases: {(cmd.Aliases?.Length < 1 ? "none" : string.Join(",", cmd.Aliases))}\n";

            return true;
        }
    }
}