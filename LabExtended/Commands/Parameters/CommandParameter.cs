using System.Reflection;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

#pragma warning disable CS8618, CS9264
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Represents a command parameter.
/// </summary>
public class CommandParameter
{
    /// <summary>
    /// Gets the parameter's type.
    /// </summary>
    public CommandParameterType Type { get; }

    /// <summary>
    /// Gets the list of parameter arguments.
    /// </summary>
    public List<ICommandParameterRestriction> Restrictions { get; } = new();
    
    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string Description { get; internal set; }
    
    /// <summary>
    /// Gets the <see cref="CommandSystem.IUsageProvider.Usage"/> alias of this parameter.
    /// </summary>
    public string? UsageAlias => Type?.Parser?.UsageAlias;

    /// <summary>
    /// Gets the type's friendly alias.
    /// </summary>
    public string? FriendlyAlias => Type?.Parser?.FriendlyAlias;

    /// <summary>
    /// Whether or not this parameter is optional.
    /// </summary>
    public bool HasDefault { get; internal set; }
    
    /// <summary>
    /// Gets the default value.
    /// </summary>
    public object? DefaultValue { get; internal set; }

    /// <summary>
    /// Creates a new <see cref="CommandParameter"/> instance.
    /// </summary>
    /// <param name="parameterInfo">The targeted parameter.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameter(ParameterInfo parameterInfo)
    {
        if (parameterInfo is null)
            throw new ArgumentNullException(nameof(parameterInfo));

        Type = new(parameterInfo);

        Name = parameterInfo.Name;
        HasDefault = parameterInfo.HasDefaultValue;
        DefaultValue = parameterInfo.DefaultValue;
        Description = "None";

        var parameterAttribute = parameterInfo.GetCustomAttribute<CommandParameterAttribute>(true);

        if (parameterAttribute == null) 
            return;
        
        Restrictions.AddRange(parameterAttribute.Restrictions);
            
        if (!string.IsNullOrWhiteSpace(parameterAttribute.Name))
            Name = parameterAttribute.Name;
            
        if (!string.IsNullOrWhiteSpace(parameterAttribute.Description))
            Description = parameterAttribute.Description;
    }

    /// <summary>
    /// Creates a new <see cref="CommandParameter"/> instance.
    /// </summary>
    /// <param name="type">The parameter type.</param>
    /// <param name="name">Parameter name.</param>
    /// <param name="description">Parameter description.</param>
    /// <param name="defaultValue">Default value.</param>
    /// <param name="hasDefault">Is the parameter optional?</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameter(Type type, string name, string description, object defaultValue, bool hasDefault)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        Type = new(type);
        
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        
        DefaultValue = defaultValue;
        HasDefault = hasDefault;
    }

    internal CommandParameter()
    {
        Type = new();

        Name = "NULL";
        Description = "NULL";

        HasDefault = false;

        DefaultValue = null;
    }

    /// <summary>
    /// Whether or not this parameter has the specified restriction.
    /// </summary>
    /// <param name="restriction">The restriction instance.</param>
    /// <typeparam name="T">The restriction type.</typeparam>
    /// <returns>true if the restriction was found</returns>
    public bool HasRestriction<T>(out T restriction) where T : ICommandParameterRestriction
    {
        for (var i = 0; i < Restrictions.Count; i++)
        {
            if (Restrictions[i] is T value)
            {
                restriction = value;
                return true;
            }
        }
        
        restriction = default;
        return false;
    }
}