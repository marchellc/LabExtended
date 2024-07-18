namespace LabExtended.Core.Commands.Interfaces
{
    public interface ICommandParser
    {
        string Name { get; }

        bool TryParse(string value, out string failureMessage, out object result);
    }
}