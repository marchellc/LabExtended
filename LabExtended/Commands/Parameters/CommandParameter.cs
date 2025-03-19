using System.ComponentModel;
using System.Reflection;

using LabExtended.Commands.Interfaces;

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
    /// Gets the name of the parameter.
    /// </summary>
    public string Name { get; internal set; }
    
    /// <summary>
    /// Gets the description of the parameter.
    /// </summary>
    public string Description { get; internal set; }

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

        var descriptionAttribute = parameterInfo.GetCustomAttribute<DescriptionAttribute>();
        
        if (descriptionAttribute != null)
            Description = descriptionAttribute.Description;

        Description ??= "None";
    }

    /// <summary>
    /// Gets a value indicating whether or not the token can be accepted by this parameter.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>true if the token is acceptable</returns>
    public bool AcceptsToken(ICommandToken token)
    {
        if (token is null)
            throw new ArgumentNullException(nameof(token));
        
        return Type.Parser.AcceptsToken(token);
    }
}