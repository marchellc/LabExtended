using CommandSystem;

namespace LabExtended.Commands
{
    public class VanillaParentCommandBase : ParentCommand
    {
        public VanillaParentCommandBase(string name, string description, params string[] aliases)
        {
            Command = name;
            Aliases = aliases;
            Description = description;

            LoadGeneratedCommands();
        }

        public override string Command { get; }
        public override string Description { get; }

        public override string[] Aliases { get; }

        public virtual bool AllowParentCall { get; }

        public virtual void OnInitialized() { }

        public virtual bool OnParentCalled(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = null;
            return false;
        }

        public override void LoadGeneratedCommands()
            => OnInitialized();

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