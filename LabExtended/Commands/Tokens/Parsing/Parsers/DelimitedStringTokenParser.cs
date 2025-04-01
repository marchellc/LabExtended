using LabExtended.Commands.Interfaces;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands.Tokens.Parsing.Parsers;

/// <summary>
/// Parses string tokens delimited by "
/// </summary>
public class DelimitedStringTokenParser : CommandTokenParser
{
    /// <inheritdoc cref="CommandTokenParser.ShouldStart"/>
    public override bool ShouldStart(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(StringToken.Token);

    /// <inheritdoc cref="CommandTokenParser.ShouldTerminate"/>
    public override bool ShouldTerminate(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(StringToken.Token);

    /// <inheritdoc cref="CommandTokenParser.ProcessContext"/>
    public override bool ProcessContext(CommandTokenParserContext context)
    {
        if (context.CurrentParser is not DelimitedStringTokenParser)
            return true;

        // Prevents a leading whitespace.
        if (context is { IsCurrentWhiteSpace: true, Builder.Length: < 1 })
            return false;

        context.Builder.Append(context.CurrentChar);
        return false;
    }

    /// <inheritdoc cref="CommandTokenParser.OnTerminated"/>
    public override void OnTerminated(CommandTokenParserContext context)
    {
        if (context.CurrentParser is not DelimitedStringTokenParser)
            return;

        if (!context.CurrentTokenIs<StringToken>(out var stringToken))
            return;
        
        // Remove all trailing whitespaces.
        while (char.IsWhiteSpace(context.Builder[context.Builder.Length - 1]))
            context.Builder.Remove(context.Builder.Length - 1, 1);
        
        stringToken.Value = context.Builder.ToString();
    }

    /// <inheritdoc cref="CommandTokenParser.CreateToken"/>
    public override ICommandToken CreateToken(CommandTokenParserContext context)
        => StringToken.Instance.NewToken();
}