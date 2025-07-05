using System.Reflection;

using LabExtended.Core;
using LabExtended.Extensions;

namespace LabExtended.Utilities;

/// <summary>
/// Utilities targeting the reflection API.
/// </summary>
public static class ReflectionUtils
{
    /// <summary>
    /// Gets called once a new type is discovered.
    /// </summary>
    public static event Action<Type>? Discovered;

    /// <summary>
    /// Gets a list of all loaded types.
    /// </summary>
    public static List<Type> Types { get; } = new();
    
    /// <summary>
    /// Gets a list of all loaded assemblies.
    /// </summary>
    public static List<Assembly> Assemblies { get; } = new();

    private static void OnLoaded(object _, AssemblyLoadEventArgs ev)
    {
        try
        {
            Assemblies.AddUnique(ev.LoadedAssembly);
            
            foreach (var type in ev.LoadedAssembly.GetTypes())
            {
                Discovered?.InvokeSafe(type);

                Types.AddUnique(type);
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Reflection Utils", $"Error while processing types in assembly &3{ev.LoadedAssembly.FullName}&r:\n{ex}");
        }
    }
    
    internal static void Load()
    {
        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    Assemblies.AddUnique(assembly);
                    
                    foreach (var type in assembly.GetTypes())
                    {
                        Discovered?.InvokeSafe(type);

                        Types.AddUnique(type);
                    }
                }
                catch (Exception ex)
                {
                    ApiLog.Error("Reflection Utils", $"Error while processing types in assembly &3{assembly.FullName}&r:\n{ex}");
                }
            }
        }
        catch (Exception ex)
        {
            ApiLog.Error("Reflection Utils", $"Error while processing assemblies:\n{ex}");
        }

        AppDomain.CurrentDomain.AssemblyLoad += OnLoaded;
    }
}