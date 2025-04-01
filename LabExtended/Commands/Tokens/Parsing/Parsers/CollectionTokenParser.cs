using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands.Tokens.Parsing.Parsers;

/// <summary>
/// Parsers collection tokens.
/// </summary>
public class CollectionTokenParser : CommandTokenParser
{
    /// <inheritdoc cref="CommandTokenParser.ShouldStart"/>
    public override bool ShouldStart(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(CollectionToken.StartToken);

    /// <inheritdoc cref="CommandTokenParser.ShouldTerminate"/>
    public override bool ShouldTerminate(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(CollectionToken.EndToken);

    /// <inheritdoc cref="CommandTokenParser.OnTerminated"/>
    public override void OnTerminated(CommandTokenParserContext context)
    {
        if (!context.CurrentTokenIs<CollectionToken>(out var collectionToken))
            return;

        if (context.Builder.Length > 0)
        {
            context.Builder.RemoveTrailingWhiteSpaces();
            
            collectionToken.Values.Add(context.Builder.ToString());
        }
    }

    /// <inheritdoc cref="CommandTokenParser.ProcessContext"/>
    public override bool ProcessContext(CommandTokenParserContext context)
    {
        if (context.CurrentParser is not CollectionTokenParser)
            return true;

        if (!context.CurrentTokenIs<CollectionToken>(out var collectionToken))
            return true;

        // Prevent a leading whitespace
        if (context is { IsCurrentWhiteSpace: true, Builder.Length: < 1 })
            return true;

        if (!context.PreviousCharIsEscape() && context.CurrentCharIs(CollectionToken.SplitToken))
        {
            context.Builder.RemoveTrailingWhiteSpaces();
            
            collectionToken.Values.Add(context.Builder.ToString());
            
            context.Builder.Clear();
            return false;
        }
        
        context.Builder.Append(context.CurrentChar);
        return false;
    }

    /// <inheritdoc cref="CommandTokenParser.CreateToken"/>
    public override ICommandToken CreateToken(CommandTokenParserContext context)
        => CollectionToken.Instance.NewToken();
}