using LabExtended.API;

namespace LabExtended.Commands.Interfaces
{
    public interface ICommandParser
    {
        string Name { get; }
        
        Dictionary<string, Func<ExPlayer, object>> PlayerProperties { get; }

        bool TryParse(ExPlayer sender, string value, out string failureMessage, out object result);
    }
}