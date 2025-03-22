using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Tokens;

using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// Parsers enums.
/// </summary>
public class EnumParameterParser : CommandParameterParser
{
    /// <summary>
    /// Creates a new <see cref="EnumParameterParser"/> instance.
    /// </summary>
    /// <param name="enumType">The enum's type.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EnumParameterParser(Type enumType)
    {
        if (enumType is null)
            throw new ArgumentNullException(nameof(enumType));

        Type = enumType;
        SupportsBitFlags = enumType.IsBitwiseEnum();
        
        foreach (var obj in Enum.GetValues(enumType))
        {
            if (obj is not Enum enumValue)
                continue;
            
            Values.Add(enumValue);
        }
    }
    
    /// <summary>
    /// This parser's targeted enum type.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// A list of all possible values.
    /// </summary>
    public List<Enum> Values { get; } = new();
    
    /// <summary>
    /// Whether or not the current enum type supports bitwise operations.
    /// </summary>
    public bool SupportsBitFlags { get; }
    
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias => $"Fixed value (or values), use \"enum {Type.Name}\" or \"enum {Type.FullName}\" to view a list";

    /// <inheritdoc cref="CommandParameterParser.AcceptsToken"/>
    public override bool AcceptsToken(ICommandToken token)
        => token is PropertyToken or StringToken;

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex,
        CommandContext context,
        CommandParameter parameter)
    {
        if (token.TryProcessProperty(context, out var property))
        {
            if (property is not Enum || property.GetType() != Type)
                return new(false, null, $"Target property is not enum \"{Type.Name}\".", parameter);

            return new(true, property, null, parameter);
        }

        var stringToken = (StringToken)token;

        try
        {
            return new(true, Enum.Parse(Type, stringToken.Value, true), null, parameter);
        }
        catch (Exception ex)
        {
            return new(false, null,
                $"String \"{stringToken.Value}\" could not be converted to enum \"{Type.Name}\": {ex.Message}.",
                parameter);
        }
    }
}