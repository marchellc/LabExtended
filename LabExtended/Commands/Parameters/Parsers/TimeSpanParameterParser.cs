using System.Globalization;

using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parses <see cref="TimeSpan"/>.
/// </summary>
public class TimeSpanParameterParser : CommandParameterParser
{
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; } = "A duration (1s / 1s 2h / 2h 1d / 1m 1M)";

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex, CommandContext context,
        CommandParameter parameter)
    {
        var sourceString = string.Empty;

        if (token is PropertyToken propertyToken)
        {
            if (propertyToken.TryProcessProperty(context, out var result) && result is TimeSpan)
                return new(true, result, null, parameter);

            sourceString = result.ToString();
        }
        else if (token is StringToken stringToken)
        {
            sourceString = stringToken.Value;
        }
        
        sourceString = sourceString.Replace(".", ",");

        var args = sourceString.Split(',');
        var time = (long)0;

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i].Trim();

            if (arg.Length < 2 || !char.IsLetter(arg.Last()))
                return new(false, null, $"Could not parse argument \"{arg}\" at position {i}", parameter);

            var numString = new string(arg.Where(a => !char.IsLetter(a)).ToArray());

            if (!long.TryParse(numString, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
                return new(false, null, $"Could not parse \"{numString}\" into a valid number (position: {i})",
                    parameter);

            var unit = arg.Last();

            if (unit is 'm')
                number = TimeSpan.FromMinutes(number).Ticks;
            else if (unit is 's' or 'S')
                number = TimeSpan.FromSeconds(number).Ticks;
            else if (unit is 'h' or 'H')
                number = TimeSpan.FromHours(number).Ticks;
            else if (unit is 'd' or 'D')
                number = TimeSpan.FromDays(number).Ticks;
            else if (unit is 'M')
                number = TimeSpan.FromDays(number * 30).Ticks;
            else if (unit is 'y' or 'Y')
                number = TimeSpan.FromDays(number * 365).Ticks;
            else
                return new(false, null, $"Argument \"{unit}\" is not a valid time unit (position: {i})", parameter);

            time += number;
        }

        return new(true, TimeSpan.FromTicks(time), null, parameter);
    }
}