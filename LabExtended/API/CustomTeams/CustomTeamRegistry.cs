using HarmonyLib;

using LabExtended.Attributes;
using LabExtended.Extensions;
using LabExtended.Utilities;

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
    public static Dictionary<Type, CustomTeamHandler> RegisteredHandlers { get; } = new();

    /// <summary>
    /// Attempts to find a custom team handler by it's type name.
    /// </summary>
    /// <param name="name">The name of the type.</param>
    /// <param name="teamHandler">The found team handler.</param>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGet<THandler>(string name, out THandler teamHandler) where THandler : CustomTeamHandler
    {
        if (string.IsNullOrEmpty(name))
        {
            teamHandler = null!;
            return false;
        }

        foreach (var pair in RegisteredHandlers)
        {
            if (string.Equals(name, pair.Value.GetType().Name, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(name, pair.Value.GetType().Name.Replace("Handler", string.Empty)))
            {
                if (pair.Value is THandler handler)
                {
                    teamHandler = handler;
                    return true;
                }
            }
        }
        
        teamHandler = null!;
        return false;
    }

    /// <summary>
    /// Attempts to find a registered handler.
    /// </summary>
    /// <param name="handler">The found handler.</param>
    /// <typeparam name="THandler">The type of handler to find.</typeparam>
    /// <returns>true if the handler was found</returns>
    public static bool TryGet<THandler>(out THandler handler) where THandler : CustomTeamHandler
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
    public static THandler Register<THandler>() where THandler : CustomTeamHandler
        => (THandler)Register(typeof(THandler));
    
    /// <summary>
    /// Registers a new handler.
    /// </summary>
    /// <param name="type">The type to register.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public static CustomTeamHandler Register(Type type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        if (RegisteredHandlers.TryGetValue(type, out var active))
            return active;

        if (Activator.CreateInstance(type) is not CustomTeamHandler handler)
            throw new Exception($"Type {type.FullName} could not be instantiated as a CustomTeamHandler");
        
        RegisteredHandlers.Add(type, handler);
        
        handler.OnRegistered();
        return handler;
    }

    private static void OnDiscovered(Type type)
    {
        if (type.HasAttribute<LoaderIgnoreAttribute>())
            return;
        
        if (!type.InheritsType<CustomTeamHandler>() || type == typeof(CustomTeamHandler))
            return;

        if (AccessTools.Constructor(type) is null)
            return;

        Register(type);
    }

    [LoaderInitialize(1)]
    private static void OnInit()
    {
        ReflectionUtils.Discovered += OnDiscovered;
    }
}