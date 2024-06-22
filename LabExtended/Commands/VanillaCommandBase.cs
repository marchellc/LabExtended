using CommandSystem;

namespace LabExtended.Commands
{
    public abstract class VanillaCommandBase : ICommand
    {
        public abstract string Command { get; }
        public virtual string Description { get; } = "No description.";

        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public virtual bool SanitizeResponse => false;

        public abstract bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response);
    }
}