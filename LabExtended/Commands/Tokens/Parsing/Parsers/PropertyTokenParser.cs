using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands.Tokens.Parsing.Parsers;

/// <summary>
/// Parses properties.
/// </summary>
public class PropertyTokenParser : CommandTokenParser
{
    /// <inheritdoc cref="CommandTokenParser.ShouldStart"/>
    public override bool ShouldStart(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape()
           && context.CurrentCharIs(PropertyToken.StartToken)
           && context.NextCharIs(PropertyToken.BracketStartToken);

    /// <inheritdoc cref="CommandTokenParser.ShouldTerminate"/>
    public override bool ShouldTerminate(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(PropertyToken.BracketEndToken);

    /// <inheritdoc cref="CommandTokenParser.OnTerminated"/>
    public override void OnTerminated(CommandTokenParserContext context)
    {
        if (!context.CurrentTokenIs<PropertyToken>(out var propertyToken))
            return;

        if (context.Builder.Length > 0)
        {
            context.Builder.RemoveTrailingWhiteSpaces();
            
            propertyToken.Name = context.Builder.ToString();
        }
    }

    /// <inheritdoc cref="CommandTokenParser.ProcessContext"/>
    public override bool ProcessContext(CommandTokenParserContext context)
    {
        if (context.CurrentParser is not PropertyTokenParser)
            return true;

        if (!context.CurrentTokenIs<PropertyToken>())
            return true;

        if (context.IsCurrentWhiteSpace && context.Builder.Length < 1)
            return false;

        if (context.CurrentCharIs(PropertyToken.BracketStartToken) && context.PreviousCharIs(PropertyToken.StartToken))
            return false;
        
        context.Builder.Append(context.CurrentChar);
        return false;
    }

    /// <inheritdoc cref="CommandTokenParser.CreateToken"/>
    public override ICommandToken CreateToken(CommandTokenParserContext context)
        => PropertyToken.Instance.NewToken();
}