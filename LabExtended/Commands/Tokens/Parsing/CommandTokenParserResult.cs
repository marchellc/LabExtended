using LabExtended.Commands.Enums;

namespace LabExtended.Commands.Tokens.Parsing;

/// <summary>
/// Represents the result of <see cref="CommandTokenParser"/>.
/// </summary>
public struct CommandTokenParserResult
{
    /// <summary>
    /// Whether or not the parsing was successful.
    /// </summary>
    public bool IsSuccess { get; }
    
    /// <summary>
    /// Gets the input string.
    /// </summary>
    public string Input { get; }

    /// <summary>
    /// Gets the error that occured.
    /// </summary>
    public string? Error { get; }
    
    /// <summary>
    /// Gets the character at which the parsing failed.
    /// </summary>
    public char? Character { get; }
    
    /// <summary>
    /// Gets the position at which the parsing failed.
    /// </summary>
    public int? Position { get; }
    
    /// <summary>
    /// Gets the stage at which the parsing failed.
    /// </summary>
    public TokenParserStage? Stage { get; }

    /// <summary>
    /// Creates a successful parsing result.
    /// </summary>
    /// <param name="input">The input string.</param>
    public CommandTokenParserResult(string input)
    {
        IsSuccess = true;
        Input = input;
    }

    /// <summary>
    /// Creates an unsuccessful parsing result.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="error">The error.</param>
    /// <param name="character">The failed character.</param>
    /// <param name="position">The failed position.</param>
    /// <param name="stage">The failed stage.</param>
    public CommandTokenParserResult(string input, string error, char character, int position, TokenParserStage stage)
    {
        IsSuccess = false;
        Input = input;
        Error = error;
        Character = character;
        Position = position;
        Stage = stage;
    }
}