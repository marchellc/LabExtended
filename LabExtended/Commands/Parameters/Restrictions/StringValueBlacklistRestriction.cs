﻿using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters.Restrictions;

/// <summary>
/// Restricts certain values from being used.
/// </summary>
public class StringValueBlacklistRestriction : ICommandParameterRestriction
{
    /// <summary>
    /// Whether or not the values are case sensitive.
    /// </summary>
    public bool IsCaseSensitive { get; }
    
    /// <summary>
    /// List of blacklisted values.
    /// </summary>
    public List<string> Values { get; }

    /// <summary>
    /// Creates a new <see cref="StringValueBlacklistRestriction"/>
    /// </summary>
    /// <param name="isCaseSensitive">Whether or not the values are case sensitive</param>
    /// <param name="values">List of blacklisted values</param>
    /// <exception cref="ArgumentNullException"></exception>
    public StringValueBlacklistRestriction(bool isCaseSensitive, IEnumerable<string> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));
        
        IsCaseSensitive = isCaseSensitive;
        Values = values as List<string> ?? values.ToList();
    }

    /// <inheritdoc cref="ICommandParameterRestriction.IsValid"/>
    public bool IsValid(object argument, CommandContext context, CommandParameter parameter, out string? error)
    {
        if (argument is not string str)
        {
            error = "Supplied argument is not a string";
            return false;
        }

        if (Values.Any(s => string.Equals(s, str,
                IsCaseSensitive ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase)))
        {
            error = $"Value \"{str}\" is blacklisted!\nBlacklisted values: \"{string.Join(", ", Values)}\"";
            return false;
        }
        
        error = null;
        return true;
    }
    
    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"String Blacklist ({Values.Count}{(IsCaseSensitive ? " - case sensitive" : "")}): {string.Join(", ", Values)}";
}