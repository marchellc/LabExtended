using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts certain values from being used.
/// </summary>
public class StringValueWhitelistRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Whether or not the values are case sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; }
    
    /// <summary>
    /// List of whitelisted values.
    /// </summary>
    public List<string> Values { get; }

    /// <summary>
    /// Initializes a new instance of the StringValueWhitelistRestriction class with an empty set of allowed string
    /// values.
    /// </summary>
    public StringValueWhitelistRestriction()
        => Values = new();

    /// <summary>
    /// Creates a new <see cref="StringValueWhitelistRestriction"/>
    /// </summary>
    /// <param name="isCaseSensitive">Whether or not the values are case sensitive</param>
    /// <param name="values">List of whitelisted values</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StringValueWhitelistRestriction(bool isCaseSensitive, IEnumerable<string> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        
        IsCaseSensitive = isCaseSensitive;
        Values = values as List<string> ?? values.ToList();
    }

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, null, out var parts))
            return false;

        Values.AddRange(parts);
        return true;
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not string str)
        {
            error = "Supplied argument is not a string";
            return false;
        }

        if (!Values.Any(s => string.Equals(s, str,
                IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)))
        {
            error = $"Value \"{str}\" is not allowed!\nAllowed values: \"{string.Join(", ", Values)}\"";
            return false;
        }
        
        error = null;
        return true;
    }

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"String Whitelist ({Values.Count}{(IsCaseSensitive ? " - case sensitive" : "")}): {string.Join(", ", Values)}";
}