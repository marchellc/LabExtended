using LabExtended.Commands.Interfaces;
using LabExtended.Extensions;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts certain values from being used.
/// </summary>
public class EnumValueWhitelistRestriction<T> : ICommandParameterRestriction where T : struct, Enum
{
    /// <summary>
    /// List of blacklisted values.
    /// </summary>
    public List<T> Values { get; }

    /// <summary>
    /// Initializes a new instance of the EnumValueWhitelistRestriction class.
    /// </summary>
    public EnumValueWhitelistRestriction()
    {
        Values = new();
    }

    /// <summary>
    /// Creates a new <see cref="EnumValueWhitelistRestriction{T}"/>
    /// </summary>
    /// <param name="values">List of whitelisted values</param>
    /// <exception cref="ArgumentNullException"></exception>
    public EnumValueWhitelistRestriction(IEnumerable<T> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        
        Values = values as List<T> ?? values.ToList();
    }

    /// <inheritdoc/>
    public bool TryLoad(string value)
    {
        if (!value.TrySplit(',', true, null, out var parts))
            return false;

        for (int i = 0; i < parts.Length; i++)
        {
            if (!Enum.TryParse<T>(parts[i], true, out var enumValue))
                return false;

            Values.Add(enumValue);
        }

        return true;
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not T value)
        {
            error = "Supplied argument is not the target enum.";
            return false;
        }

        if (!Values.Contains(value))
        {
            error = $"Value \"{value}\" is not allowed!\nAllowed values: \"{string.Join(", ", Values)}\"";
            return false;
        }
        
        error = null;
        return true;
    }
    
    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"Enum Whitelist ({Values.Count}): {string.Join(", ", Values)}";
}