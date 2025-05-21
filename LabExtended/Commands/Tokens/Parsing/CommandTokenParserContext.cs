using System.Text;

using LabExtended.Commands.Interfaces;
using LabExtended.Core;
using LabExtended.Extensions;
using NorthwoodLib.Pools;

namespace LabExtended.Commands.Tokens.Parsing;

/// <summary>
/// Contains data about the current token parsing context.
/// </summary>
public class CommandTokenParserContext
{
    /// <summary>
    /// Gets the previous token.
    /// </summary>
    public ICommandToken? PreviousToken { get; private set; }
    
    /// <summary>
    /// Gets the current token.
    /// </summary>
    public ICommandToken? CurrentToken { get; internal set; }
 
    /// <summary>
    /// Gets the token collection.
    /// </summary>
    public IList<ICommandToken> Tokens { get; }
    
    /// <summary>
    /// Gets the builder assigned to this context.
    /// </summary>
    public StringBuilder? Builder { get; internal set; }
    
    /// <summary>
    /// Gets the current token's parser.
    /// </summary>
    public CommandTokenParser? CurrentParser { get; internal set; }
    
    /// <summary>
    /// Gets the parser of the previous token.
    /// </summary>
    public CommandTokenParser? PreviousParser { get; private set; }
    
    /// <summary>
    /// Gets the previous character.
    /// </summary>
    public char? PreviousChar { get; internal set; }
    
    /// <summary>
    /// Gets the next character.
    /// </summary>
    public char? NextChar { get; internal set; }
    
    /// <summary>
    /// Gets the current character.
    /// </summary>
    public char CurrentChar { get; internal set; }
    
    /// <summary>
    /// Gets the input string.
    /// </summary>
    public string Input { get; }
    
    /// <summary>
    /// Gets the current index.
    /// </summary>
    public int Index { get; internal set; }

    /// <summary>
    /// Gets the amount of parameters in the target overload.
    /// </summary>
    public int Parameters { get; }

    /// <summary>
    /// Gets or sets the custom state.
    /// </summary>
    public object? State { get; set; }

    /// <summary>
    /// Whether or not the current position is the end of the input string.
    /// </summary>
    public bool IsEnd => !NextChar.HasValue;

    /// <summary>
    /// Whether or not the current character is a whitespace.
    /// </summary>
    public bool IsCurrentWhiteSpace => char.IsWhiteSpace(CurrentChar);

    /// <summary>
    /// Whether or not the previous character is a whitespace.
    /// </summary>
    public bool IsPreviousWhiteSpace => PreviousChar.HasValue && char.IsWhiteSpace(PreviousChar.Value);
    
    /// <summary>
    /// Whether or not the next character is a whitespace.
    /// </summary>
    public bool IsNextWhiteSpace => NextChar.HasValue && char.IsWhiteSpace(NextChar.Value);
    
    /// <summary>
    /// Creates a new <see cref="CommandTokenParserContext"/> instance.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="tokens">The result token list.</param>
    /// <param name="parameters">The amount of parameters in the target overload.</param>
    public CommandTokenParserContext(string input, IList<ICommandToken> tokens, int parameters)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        
        Parameters = parameters;
        
        Builder = StringBuilderPool.Shared.Rent();
    }
    
    /// <summary>
    /// Whether or not the current character is a specific one.
    /// </summary>
    /// <param name="c">The expected character.</param>
    /// <returns>true if the current character is equal to <paramref name="c"/>.</returns>
    public bool CurrentCharIs(char c) 
        => CurrentChar == c;
    
    /// <summary>
    /// Whether or not the next character is a specific one.
    /// </summary>
    /// <param name="c">The expected character.</param>
    /// <returns>true if the next character is equal to <paramref name="c"/>.</returns>
    public bool NextCharIs(char c) 
        => NextChar == c;
    
    /// <summary>
    /// Whether or not the previous character is a specific one.
    /// </summary>
    /// <param name="c">The expected character.</param>
    /// <returns>true if the previous character is equal to <paramref name="c"/>.</returns>
    public bool PreviousCharIs(char c) 
        => PreviousChar == c;

    /// <summary>
    /// Whether or not the previous character is the escape token.
    /// </summary>
    /// <returns>true if the previous character is the escape token</returns>
    public bool PreviousCharIsEscape()
        => PreviousChar == CommandTokenParserUtils.EscapeToken;
    
    /// <summary>
    /// Whether or not the current token is a specific type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>true if the current token is a specific type</returns>
    public bool CurrentTokenIs<T>() where T : ICommandToken
        => CurrentToken is T;

    /// <summary>
    /// Whether or not the current token is a specific type.
    /// </summary>
    /// <param name="token">The resulting token.</param>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>true if the current token is a specific type</returns>
    public bool CurrentTokenIs<T>(out T? token) where T : ICommandToken
    {
        token = default;

        if (CurrentToken is not T result)
            return false;

        token = result;
        return true;
    }
    
    /// <summary>
    /// Whether or not the previous token is a specific type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>true if the previous token is a specific type</returns>
    public bool PreviousTokenIs<T>() where T : ICommandToken
        => PreviousToken is T;

    /// <summary>
    /// Whether or not the previous token is a specific type.
    /// </summary>
    /// <param name="token">The resulting token.</param>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <returns>true if the previous token is a specific type</returns>
    public bool PreviousTokenIs<T>(out T? token) where T : ICommandToken
    {
        token = default;

        if (PreviousToken is not T result)
            return false;

        token = result;
        return true;
    }

    /// <summary>
    /// Whether or not the current <see cref="State"/> is of the specific type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>true if the current state implements the type</returns>
    public bool StateIs<T>()
        => State is T;

    /// <summary>
    /// Whether or not the current <see cref="State"/> is of the specific type.
    /// </summary>
    /// <param name="state">The cast state.</param>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>true if the current state implements the type</returns>
    public bool StateIs<T>(out T? state)
    {
        state = default;
        
        if (State is not T result)
            return false;
        
        state = result;
        return true;
    }

    /// <summary>
    /// Terminates the current token.
    /// </summary>
    /// <param name="overrideParser">Whether or not to skip parser checking.</param>
    /// <param name="addToken">Whether or not the token should be added to the list of tokens.</param>
    /// <returns>true if the token was terminated</returns>
    public bool TerminateToken(bool overrideParser = false, bool addToken = true)
    {
        if (CurrentToken is null)
            return false;
        
        if (!overrideParser && CurrentParser != null && !CurrentParser.OnTerminating(this))
            return false;
        
        CurrentParser?.OnTerminated(this);

        Builder?.Clear();

        if (addToken)
        {
            Tokens.Add(CurrentToken);

            PreviousToken = CurrentToken;
            PreviousParser = CurrentParser;
        }

        CurrentToken = null;
        CurrentParser = null;

        return true;
    }

    /// <summary>
    /// Prints a status message into the server console.
    /// </summary>
    public void PrintToConsole()
    {
        ApiLog.Debug("Command Token Parser", 
            $"\n&3Previous Token&r: &6{PreviousToken?.GetType().Name ?? "null"}&r\n" +
            $"&3Previous Parser&r: &6{PreviousParser?.GetType().Name ?? "null"}&r\n" +
            $"&3Previous Character&r: &6{PreviousChar?.ToString() ?? "null"}\n" +
            $"&3Current Token&r: &6{CurrentToken?.GetType().Name ?? "null"}\n" +
            $"&3Current Parser&r: &6{CurrentParser?.GetType().Name ?? "null"}\n" +
            $"&3Current Character&r: &6{CurrentChar}&r\n" +
            $"&3Next Character&r: &6{NextChar}&r\n" +
            $"&3Builder Size&r: &6{Builder.Length}&r\n" +
            $"&3Position&r: &6{Index}&r / &6{Input.Length}&r\n" +
            $"&3Tokens&r: &6{Tokens.Count}&r\n" +
            $"&3State&r: &6{State?.GetType().Name ?? "null"}\n" +
            $"&3Is Current / Previous / Next WhiteSpace&r: &6{IsCurrentWhiteSpace} / {IsPreviousWhiteSpace} / {IsNextWhiteSpace}&r\n" +
            $"&3Is End&r: &6{IsEnd}&r");
    }
}