using LabExtended.API.CustomTeams.Internal;

namespace LabExtended.API.CustomTeams;

// There may be a lot of bugs in this API, I was literally falling asleep while writing this ..

/// <summary>
/// Contains all registered custom teams.
/// </summary>
public static class CustomTeamRegistry
{
    /// <summary>
    /// Gets a list of all registered handlers.
    /// </summary>
    public static Dictionary<Type, object> RegisteredHandlers { get; } = new();

    /// <summary>
    /// Attempts to find a registered handler.
    /// </summary>
    /// <param name="handler">The found handler.</param>
    /// <typeparam name="THandler">The type of handler to find.</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGet<THandler>(out THandler handler) where THandler : Internal_CustomTeamHandlerBase
    {
        if (RegisteredHandlers.TryGetValue(typeof(THandler), out var result))
        {
            handler = (THandler)result;
            return true;
        }
        
        handler = null!;
        return false;
    }

    /// <summary>
    /// Registers a new handler.
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static THandler Register<THandler>() where THandler : Internal_CustomTeamHandlerBase
        => (THandler)Register(typeof(THandler));
    
    /// <summary>
    /// Registers a new handler.
    /// </summary>
    /// <param name="type">The type to register.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static object Register(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (RegisteredHandlers.TryGetValue(type, out var active))
            return active;

        if (Activator.CreateInstance(type) is not Internal_CustomTeamHandlerBase handler)
            throw new Exception($"Type {type.FullName} could not be instantiated as a CustomTeamHandler");
        
        RegisteredHandlers.Add(type, handler);
        
        handler.OnRegistered();
        return handler;
    }
}