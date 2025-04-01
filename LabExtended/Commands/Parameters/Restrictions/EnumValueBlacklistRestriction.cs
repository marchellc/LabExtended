using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts certain values from being used.
/// </summary>
public class EnumValueBlacklistRestriction<T> : ICommandParameterRestriction where T : Enum
{
    /// <summary>
    /// List of blacklisted values.
    /// </summary>
    public List<T> Values { get; }

    /// <summary>
    /// Creates a new <see cref="EnumValueBlacklistRestriction{T}"/>
    /// </summary>
    /// <param name="values">List of blacklisted values</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EnumValueBlacklistRestriction(IEnumerable<T> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        
        Values = values as List<T> ?? values.ToList();
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not T value)
        {
            error = "Supplied argument is not the target enum.";
            return false;
        }

        if (Values.Contains(value))
        {
            error = $"Value \"{value}\" is blacklisted!\nBlacklisted values: \"{string.Join(", ", Values)}\"";
            return false;
        }
        
        error = null;
        return true;
    }
    
    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"Enum Blacklist ({Values.Count}): {string.Join(", ", Values)}";
}