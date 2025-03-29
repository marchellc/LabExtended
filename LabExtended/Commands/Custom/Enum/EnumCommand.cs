using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Parameters;
using LabExtended.Commands.Parameters.Parsers;

namespace LabExtended.Commands.Custom.Enum;

/// <summary>
/// Displays all enum values.
/// </summary>
[Command("enum", "Displays enum values.")]
public class EnumCommand : CommandBase, IAllCommand
{
    /// <summary>
    /// Invokes the command.
    /// </summary>
    /// <param name="enumName">Name of the enum</param>
    [CommandOverload]
    public void Invoke(string enumName)
    {
        var enumParser = default(EnumParameterParser);

        foreach (var parser in CommandParameterParserUtils.Parsers)
        {
            if (parser.Value is not EnumParameterParser enumParameterParser)
                continue;
            
            if (string.Equals(enumName, parser.Key.FullName, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(enumName, parser.Key.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                enumParser = enumParameterParser;
                break;
            }
        }

        if (enumParser is null)
        {
            Fail($"Unknown enum type: \"{enumName}\"");
            return;
        }
        
        Ok(x =>
        {
            x.AppendLine($"Enum \"{enumParser.Type.FullName}\":");

            if (enumParser.SupportsBitFlags)
                x.AppendLine($"- Supports flags");

            for (var i = 0; i < enumParser.Values.Count; i++)
            {
                var value = enumParser.Values[i];
                var numeric = Convert.ChangeType(value, System.Enum.GetUnderlyingType(enumParser.Type));

                x.AppendLine($"[{numeric}] {value}");
            }
        });
    }
}