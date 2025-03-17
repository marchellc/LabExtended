namespace LabExtended.Commands.Parameters;

/// <summary>
/// Builds command parameters.
/// </summary>
public class CommandParameterBuilder
{
    /// <summary>
    /// The built command parameter.
    /// </summary>
    public CommandParameter Result { get; } = new();

    /// <summary>
    /// Sets the parameter's type.
    /// </summary>
    /// <param name="parameterType">The type to set.</param>
    /// <returns>This builder instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public CommandParameterBuilder WithType(Type parameterType)
    {
        if (parameterType is null)
            throw new ArgumentNullException(nameof(parameterType));
        
        Result.Type = parameterType;
        return this;
    }

    /// <summary>
    /// Sets the parameter's type.
    /// </summary>
    /// <typeparam name="T">The type to set.</typeparam>
    /// <returns>This builder instance.</returns>
    public CommandParameterBuilder WithType<T>()
    {
        Result.Type = typeof(T);
        return this;
    }
}