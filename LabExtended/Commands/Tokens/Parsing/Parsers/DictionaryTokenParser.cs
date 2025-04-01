using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace LabExtended.Commands.Tokens.Parsing.Parsers;

/// <summary>
/// Parses dictionary tokens.
/// </summary>
public class DictionaryTokenParser : CommandTokenParser
{
    /// <inheritdoc cref="CommandTokenParser.ShouldStart"/>
    public override bool ShouldStart(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() 
           && context.CurrentCharIs(DictionaryToken.StartToken) 
           && !context.PreviousCharIs(PropertyToken.StartToken);

    /// <inheritdoc cref="CommandTokenParser.ShouldTerminate"/>
    public override bool ShouldTerminate(CommandTokenParserContext context)
        => !context.PreviousCharIsEscape() && context.CurrentCharIs(DictionaryToken.EndToken);

    /// <inheritdoc cref="CommandTokenParser.OnTerminated"/>
    public override void OnTerminated(CommandTokenParserContext context)
    {
        if (!context.CurrentTokenIs<DictionaryToken>(out var dictionaryToken))
            return;

        if (dictionaryToken.KeyBuilder.Length < 1 || dictionaryToken.ValueBuilder.Length < 1)
            return;

        dictionaryToken.KeyBuilder.RemoveTrailingWhiteSpaces();
        dictionaryToken.ValueBuilder.RemoveTrailingWhiteSpaces();
        
        var key = dictionaryToken.KeyBuilder.ToString();
        var value = dictionaryToken.ValueBuilder.ToString();
        
        dictionaryToken.KeyBuilder.Clear();
        dictionaryToken.ValueBuilder.Clear();
        
        if (!dictionaryToken.Values.ContainsKey(key))
            dictionaryToken.Values.Add(key, value);
    }

    /// <inheritdoc cref="CommandTokenParser.ProcessContext"/>
    public override bool ProcessContext(CommandTokenParserContext context)
    {
        if (context.CurrentParser is not DictionaryTokenParser)
            return true;

        if (!context.CurrentTokenIs<DictionaryToken>(out var dictionaryToken))
            return true;

        if (!dictionaryToken.IsValue)
        {
            if (context.IsCurrentWhiteSpace && dictionaryToken.KeyBuilder.Length < 1) 
                return false;

            if (context.CurrentCharIs(DictionaryToken.SplitToken) && !context.PreviousCharIsEscape())
            {
                dictionaryToken.IsValue = true;
                return false;
            }
            
            dictionaryToken.KeyBuilder.Append(context.CurrentChar);
            return false;
        }

        if (context.IsCurrentWhiteSpace && dictionaryToken.ValueBuilder.Length < 1)
            return false;
        
        if (context.CurrentCharIs(CollectionToken.SplitToken) && !context.PreviousCharIsEscape())
        {
            dictionaryToken.IsValue = false;

            dictionaryToken.KeyBuilder.RemoveTrailingWhiteSpaces();
            dictionaryToken.ValueBuilder.RemoveTrailingWhiteSpaces();

            var key = dictionaryToken.KeyBuilder.ToString();
            var value = dictionaryToken.ValueBuilder.ToString();

            if (!dictionaryToken.Values.ContainsKey(key))
                dictionaryToken.Values.Add(key, value);

            dictionaryToken.KeyBuilder.Clear();
            dictionaryToken.ValueBuilder.Clear();

            return false;
        }
        
        dictionaryToken.ValueBuilder.Append(context.CurrentChar);
        return false;
    }

    /// <inheritdoc cref="CommandTokenParser.CreateToken"/>
    public override ICommandToken CreateToken(CommandTokenParserContext context)
        => DictionaryToken.Instance.NewToken();
}