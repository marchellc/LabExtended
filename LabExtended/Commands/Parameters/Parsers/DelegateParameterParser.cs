using LabExtended.Commands.Tokens;
using LabExtended.Commands.Contexts;
using LabExtended.Commands.Utilities;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Parsers;

/// <summary>
/// A parser that uses base type delegates to convert.
/// </summary>
public class DelegateParameterParser<T> : CommandParameterParser
{
    /// <summary>
    /// Used to parse a string to a value.
    /// </summary>
    public delegate bool TryParseDelegate(string value, out string error, out T result);
    
    /// <summary>
    /// Creates a new <see cref="DelegateParameterParser{T}"/> instance.
    /// </summary>
    /// <param name="parserDelegate">The delegate used to parse.</param>
    /// <param name="friendlyAlias">The friendly type alias.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public DelegateParameterParser(TryParseDelegate parserDelegate, string? friendlyAlias = null)
    {
        ParserDelegate = parserDelegate ?? throw new ArgumentNullException(nameof(parserDelegate));
        FriendlyAlias = friendlyAlias ?? typeof(T).Name;
    }
    
    /// <inheritdoc cref="CommandParameterParser.FriendlyAlias"/>
    public override string? FriendlyAlias { get; }
    
    /// <summary>
    /// Gets the parser delegate.
    /// </summary>
    public TryParseDelegate ParserDelegate { get; }

    /// <inheritdoc cref="CommandParameterParser.Parse"/>
    public override CommandParameterParserResult Parse(List<ICommandToken> tokens, ICommandToken token, int tokenIndex,
        CommandContext context,
        CommandParameter parameter)
    {
        var sourceString = string.Empty;

        if (token is PropertyToken propertyToken
            && propertyToken.TryGet<object>(context, null, out var result))
        {
            if (result.GetType() == parameter.Type.Type)
                return new(true, result, null, parameter);

            if (result is string str)
                sourceString = str;
            else
                return new(false, null, $"Unsupported property type: {result.GetType().FullName}", parameter);
        }
        else if (token is StringToken stringToken)
        {
            sourceString = stringToken.Value;
        }
        else
        {
            return new(false, null, $"Unsupported token: {token.GetType().Name}", parameter);
        }

        if (!ParserDelegate(sourceString, out var error, out var value))
            return new(false, null, error, parameter);

        return new(true, value, null, parameter);
    }
}