namespace LabExtended.Core.Commands
{
    public class CommandInfo
    {
        public CommandParsingFlags ParsingFlags { get; }
        public CommandParameter[] Parameters { get; }

        public bool ShouldIgnoreSurplus => (ParsingFlags & CommandParsingFlags.IgnoreSurplus) != 0;
    }
}
