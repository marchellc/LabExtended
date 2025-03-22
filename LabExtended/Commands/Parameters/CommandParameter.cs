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
    public List<ICommandParameterArgument> Arguments { get; } = new();
    
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

        var parameterAttribute = parameterInfo.GetCustomAttribute<CommandParameterAttribute>();

        if (parameterAttribute != null)
        {
            Arguments.AddRange(parameterAttribute.Arguments);
            
            if (!string.IsNullOrWhiteSpace(parameterAttribute.Name))
                Name = parameterAttribute.Name;
            
            if (!string.IsNullOrWhiteSpace(parameterAttribute.Description))
                Description = parameterAttribute.Description;
        }
    }
}