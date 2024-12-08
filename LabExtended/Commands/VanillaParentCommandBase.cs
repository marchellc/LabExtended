using CommandSystem;

namespace LabExtended.Commands
{
    public class VanillaParentCommandBase : ParentCommand
    {
        public VanillaParentCommandBase()
            => LoadGeneratedCommands();

        public override string Command { get; } = string.Empty;
        public override string Description { get; } = string.Empty;

        public override string[] Aliases { get; } = Array.Empty<string>();

        public virtual bool AllowParentCall { get; } = false;

        public override void LoadGeneratedCommands() { }

        public virtual bool OnParentCalled(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (AllowParentCall)
                return OnParentCalled(arguments, sender, out response);

            response = "Please use one of these valid subcommands:\n";

            foreach (var cmd in Commands.Values)
                response += $"- {Command} {cmd.Command} ({cmd.Description}) | Aliases: {(cmd.Aliases?.Length < 1 ? "none" : string.Join(",", cmd.Aliases))}\n";

            return true;
        }
    }
}