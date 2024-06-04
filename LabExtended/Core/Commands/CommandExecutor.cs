namespace LabExtended.Core.Commands
{
    public class CommandExecutor
    {
        public virtual void Execute(CommandContext ctx, Action callback) { }
    }
}