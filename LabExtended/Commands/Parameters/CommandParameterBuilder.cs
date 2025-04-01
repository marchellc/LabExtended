using System.Reflection;

using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Parameters;

/// <summary>
/// Builds command parameters.
/// </summary>
public class CommandParameterBuilder
{
    /// <summary>
    /// The built command parameter.
    /// </summary>
    public CommandParameter Result { get; }

    /// <summary>
    /// Creates a new <see cref="CommandParameterBuilder"/> instance.
    /// </summary>
    /// <param name="parameterInfo">The target parameter.</param>
    public CommandParameterBuilder(ParameterInfo parameterInfo)
    {
        if (parameterInfo is null)
            throw new ArgumentNullException(nameof(parameterInfo));
        
        Result = new(parameterInfo);
    }

    /// <summary>
    /// Sets the name of the parameter.
    /// </summary>
    /// <param name="name">The parameter's name.</param>
    /// <returns>This builder instance.</returns>
    public CommandParameterBuilder WithName(string name)
    {
        Result.Name = name;
        return this;
    }

    /// <summary>
    /// Sets the description of the parameter.
    /// </summary>
    /// <param name="description">The parameter's description.</param>
    /// <returns>This builder instance.</returns>
    public CommandParameterBuilder WithDescription(string description)
    {
        Result.Description = description;
        return this;
    }

    /// <summary>
    /// Sets the parameter's default value.
    /// </summary>
    /// <param name="defaultValue">The default value to set.</param>
    /// <returns>This builder instance.</returns>
    public CommandParameterBuilder WithDefault(object? defaultValue = null)
    {
        Result.HasDefault = true;
        Result.DefaultValue = defaultValue;
        
        return this;
    }

    /// <summary>
    /// Adds a new restriction to this parameter.
    /// </summary>
    /// <typeparam name="T">The argument's type.</typeparam>
    /// <returns>This builder instance.</returns>
    public CommandParameterBuilder WithRestriction<T>() where T : ICommandParameterRestriction, new()
    {
        if (Result.Restrictions.Any(x => x is T))
            throw new Exception($"Only a single parameter argument of a type is allowed ({typeof(T).FullName})");
        
        Result.Restrictions.Add(new T());
        return this;
    }
    
    /// <summary>
    /// Adds a new restriction to this parameter.
    /// </summary>
    /// <param name="parameterRestriction">The argument.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameterBuilder WithRestriction(ICommandParameterRestriction parameterRestriction)
    {
        if (parameterRestriction is null)
            throw new ArgumentNullException(nameof(parameterRestriction));
        
        Result.Restrictions.Add(parameterRestriction);
        return this;
    }
}