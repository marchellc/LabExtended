namespace LabExtended.Core.Commands.Parsing
{
    public struct NamedArgument
    {
        public string ArgumentName { get; }

        public NamedArgument(string argName)
            => ArgumentName = argName;
    }
}