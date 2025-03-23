namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents the result of a parsed argument.
/// </summary>
public struct CommandParameterParserResult
{
    /// <summary>
    /// Whether or not the value was parsed.
    /// </summary>
    public bool Success { get; }
    
    /// <summary>
    /// The parsed value.
    /// </summary>
    public object? Value { get; }
    
    /// <summary>
    /// The error message.
    /// </summary>
    public string? Error { get; }
    
    /// <summary>
    /// The targeted parameter.
    /// </summary>
    public CommandParameter? Parameter { get; }

    /// <summary>
    /// Creates a new <see cref="CommandParameterParserResult"/> instance.
    /// </summary>
    /// <param name="success">Whether or not the value was parsed.</param>
    /// <param name="value">The parsed value.</param>
    /// <param name="error">The parsing error.</param>
    /// <param name="parameter">The failed parameter.</param>
    public CommandParameterParserResult(bool success, object? value, string? error, CommandParameter parameter)
    {
        Success = success;
        Value = value;
        Error = error;
        Parameter = parameter;
    }
}