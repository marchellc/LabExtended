using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Tokens;

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter
{
    private Type? type;

    /// <summary>
    /// Gets the type of the parameter.
    /// </summary>
    public Type? Type
    {
        get => type;
        set
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            
            if (!CommandParameterParser.TryGetParser(value, out var parser))
                throw new Exception($"No parameter parsers were registered for type {value.FullName}");
            
            type = value;
            
            IsString = value == typeof(string);
            
            Parser = parser;
        }
    }
    
    /// <summary>
    /// Gets a value indicating whether or not the <see cref="Type"/> is a string.
    /// </summary>
    public bool IsString { get; private set; }
    
    /// <summary>
    /// Whether or not this parameter is optional.
    /// </summary>
    public bool HasDefault { get; internal set; }
    
    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object? DefaultValue { get; internal set; }
    
    /// <summary>
    /// Gets the parameter's parser.
    /// </summary>
    public CommandParameterParser? Parser { get; private set; }

    /// <summary>
    /// Gets a value indicating whether or not the token can be accepted by this parameter.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>true if the token is acceptable</returns>
    public bool AcceptsToken(ICommandToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));

        if (IsString && token is StringToken)
            return true;
        
        return Parser.AcceptsToken(token);
    }
}