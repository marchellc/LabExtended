using LabExtended.Extensions;

using System.Diagnostics;
using System.Reflection;

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
    /// Gets the loaded Assembly-CSharp.
    /// </summary>
    public static Assembly GameAssembly { get; } = typeof(ReferenceHub).Assembly;

    /// <summary>
    /// Gets the loaded assembly of the HarmonyLib library.
    /// </summary>
    public static Assembly HarmonyAssembly { get; } = typeof(HarmonyLib.Harmony).Assembly;

    /// <summary>
    /// Gets the loaded assembly of the Mirror library.
    /// </summary>
    public static Assembly MirrorAssembly { get; } = typeof(Mirror.NetworkIdentity).Assembly;

    /// <summary>
    /// Gets the loadded assembly of LabAPI.
    /// </summary>
    public static Assembly LabApiAssembly { get; } = typeof(LabApi.Loader.PluginLoader).Assembly;

    /// <summary>
    /// Gets a list of all loaded types.
    /// </summary>
    public static List<Type> Types { get; } = new();
    
    /// <summary>
    /// Gets a list of all loaded assemblies.
    /// </summary>
    public static List<Assembly> Assemblies { get; } = new();

    /// <summary>
    /// Gets the assembly of the calling method in the current call stack, with options to skip frames and filter
    /// assemblies.
    /// </summary>
    /// <remarks>This method can be used to identify the assembly that invoked the current code, which is
    /// useful for scenarios such as plugin discovery or diagnostics. The skipFrameCount parameter allows you to control
    /// how many stack frames to skip, which can be helpful when wrapping this method in utility functions. If
    /// ignoreAssembly is provided, any assemblies for which the predicate returns true will be skipped when searching
    /// for the caller.</remarks>
    /// <param name="skipFrameCount">The number of stack frames to skip before determining the caller assembly. Must be zero or greater.</param>
    /// <param name="throwIfNotFound">true to throw an exception if no suitable assembly is found; otherwise, false.</param>
    /// <param name="ignoreAssembly">A predicate used to exclude specific assemblies from consideration. If null, no assemblies are ignored.</param>
    /// <returns>The assembly of the first calling method in the stack that is not ignored by the specified predicate.</returns>
    /// <exception cref="Exception">Thrown if no suitable calling assembly is found and throwIfNotFound is true.</exception>
    public static Assembly GetCallerAssembly(int skipFrameCount, bool throwIfNotFound, Predicate<Assembly>? ignoreAssembly = null)
    {
        var frames = new StackTrace().GetFrames();

        for (var i = 0 + skipFrameCount; i < frames.Length; i++)
        {
            var method = frames[i].GetMethod();

            if (method is null)
                continue;

            var assembly = method.DeclaringType?.Assembly ?? method.ReflectedType.Assembly;

            if (ignoreAssembly is null || !ignoreAssembly(assembly))
                return assembly;
        }

        if (throwIfNotFound)
            throw new Exception("Could not find caller assembly.");

        return null!;
    }

    private static void OnLoaded(object _, AssemblyLoadEventArgs ev)
    {
        try
        {
            Assemblies.AddUnique(ev.LoadedAssembly);
            
            foreach (var type in ev.LoadedAssembly.GetTypes())
            {
                try
                {
                    Discovered?.Invoke(type);
                }
                catch
                {
                    // ignored
                }

                Types.AddUnique(type);
            }
        }
        catch
        {
            // ignored
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
                        try
                        {
                            Discovered?.Invoke(type);
                        }
                        catch
                        {
                            // ignored
                        }

                        Types.AddUnique(type);
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch
        {
            // ignored
        }

        AppDomain.CurrentDomain.AssemblyLoad += OnLoaded;
    }
}