using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Tokens.Parsing.Parsers;

using LabExtended.Core;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Tokens.Parsing;

/// <summary>
/// Used to parse command lines to tokens.
/// </summary>
public static class CommandTokenParserUtils
{
    /// <summary>
    /// Gets the escape token character.
    /// </summary>
    public static char EscapeToken { get; set; } = '\\';
    
    /// <summary>
    /// Gets a list of all token parsers.
    /// <remarks>Please use <see cref="List{T}.Insert"/> when adding new parsers as <see cref="LastStringTokenParser"/>
    /// needs to be the last one in order to work.</remarks>
    /// </summary>
    public static List<CommandTokenParser> Parsers { get; } =
    [
        new PropertyTokenParser(),
        
        new CollectionTokenParser(),
        new DictionaryTokenParser(),
        
        new DelimitedStringTokenParser(),
        new LastStringTokenParser()
    ];

    /// <summary>
    /// Parsers an input string into a list of tokens.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="tokens">The result list of tokens.</param>
    /// <param name="parameterCount">The amount of parameters in the command's overload.</param>
    /// <returns>The parsing result.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static bool TryParse(string input, IList<ICommandToken> tokens, int parameterCount)
    {
        if (input is null)
            throw new ArgumentNullException(nameof(input));
        
        if (tokens is null)
            throw new ArgumentNullException(nameof(tokens));
        
        if (parameterCount < 0)
            throw new ArgumentOutOfRangeException(nameof(parameterCount));
        
        try
        {
            var context = new CommandTokenParserContext(input, tokens, parameterCount);

            for (var i = 0; i < input.Length; i++)
            {
                // Set context variables
                context.Index = i;
                context.CurrentChar = input[i];

                if (i - 1 >= 0)
                    context.PreviousChar = input[i - 1];
                else
                    context.PreviousChar = null;
                
                if (i + 1 < input.Length)
                    context.NextChar = input[i + 1];
                else
                    context.NextChar = null;

                // Process the currently active parser.
                if (context.CurrentParser != null)
                {
                    // We likely hit an ending token of a parser, so just skip to the next one.
                    if (context.CurrentParser.ShouldTerminate(context))
                    {
                        context.TerminateToken();
                        continue;
                    }
                    
                    if (!context.CurrentParser.ProcessContext(context))
                    {
                        continue;
                    }
                }

                // Process other inactive parsers
                for (var x = 0; x < Parsers.Count; x++)
                {
                    var parser = Parsers[x];

                    // Handle the start of new parsers
                    if (parser.ShouldStart(context))
                    {
                        context.TerminateToken();
                        
                        context.CurrentParser = parser;
                        context.CurrentToken = parser.CreateToken(context);
                        
                        break;
                    }

                    if (!parser.ProcessContext(context))
                        break;
                }
            }

            context.TerminateToken();

            if (context.Builder != null)
                StringBuilderPool.Shared.Return(context.Builder);
            
            context.Builder = null;
        }
        catch (Exception ex)
        {
            ApiLog.Error("Command Token Parser", ex);
            return false;
        }

        return true;
    }
}