namespace LabExtended.Commands.Attributes;

/// <summary>
/// Used to mark command properties.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class CommandPropertyAliasAttribute : Attribute
{
    /// <summary>
    /// Gets the alias.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Creates a new <see cref="CommandPropertyAliasAttribute"/> instance.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandPropertyAliasAttribute(string alias)
    {
        if (string.IsNullOrWhiteSpace(alias))
            throw new ArgumentNullException(nameof(alias));
        
        Alias = alias;
    }
}