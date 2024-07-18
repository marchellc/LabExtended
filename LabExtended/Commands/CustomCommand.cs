using CommandSystem;

using LabExtended.API;
using LabExtended.Commands.Arguments;
using LabExtended.Core.Commands.Interfaces;

namespace LabExtended.Commands
{
    public class CustomCommand : ICommand
    {
        public virtual string Command { get; }
        public virtual string Description { get; }

        public virtual string[] Aliases { get; } = Array.Empty<string>();

        public virtual bool SanitizeResponse => false;

        public virtual ArgumentDefinition[] Arguments { get; }

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            throw new NotImplementedException();
        }

        public virtual ICommandResponse OnCommand(ExPlayer sender, ICommandContext content)
        {

        }
    }
}