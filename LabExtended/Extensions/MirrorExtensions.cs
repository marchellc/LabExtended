using Mirror;

#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting the Mirror library.
/// </summary>
public static class MirrorExtensions
{
    /// <summary>
    /// Whether or not a specific identity has a specific behaviour type.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>true if a behaviour of type T was found</returns>
    public static bool HasBehaviour<T>(this NetworkIdentity identity)
        => TryGetBehaviour<T>(identity, out _);
    
    /// <summary>
    /// Whether or not a specific identity has a specific behaviour type.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <param name="predicate">The filtering predicate.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>true if a behaviour of type T was found</returns>
    public static bool HasBehaviour<T>(this NetworkIdentity identity, Predicate<T> predicate)
        => TryGetBehaviour<T>(identity, predicate, out _);

    /// <summary>
    /// Gets a network behaviour of a specific type on a given network identity.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>The found behaviour instance (or null if not found)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetBehaviour<T>(this NetworkIdentity identity)
    {
        return TryGetBehaviour<T>(identity, out var behaviour) ? behaviour : default;
    }
    
    /// <summary>
    /// Gets a network behaviour of a specific type on a given network identity.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <param name="predicate">The filtering predicate.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>The found behaviour instance (or null if not found)</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static T GetBehaviour<T>(this NetworkIdentity identity, Predicate<T> predicate)
    {
        return TryGetBehaviour(identity, predicate, out var behaviour) ? behaviour : default;
    }
    
    /// <summary>
    /// Attempts to find a network behaviour of a specific type on a given network identity.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <param name="behaviour">The found behaviour instance.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>true if the behaviour instance was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetBehaviour<T>(this NetworkIdentity identity, out T behaviour)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));

        if (identity.NetworkBehaviours?.Length < 1)
        {
            behaviour = default;
            return false;
        }

        for (var i = 0; i < identity.NetworkBehaviours.Length; i++)
        {
            if (identity.NetworkBehaviours[i] is T target)
            {
                behaviour = target;
                return true;
            }
        }
        
        behaviour = default;
        return false;
    }
    
    /// <summary>
    /// Attempts to find a network behaviour of a specific type on a given network identity.
    /// </summary>
    /// <param name="identity">The target identity.</param>
    /// <param name="predicate">The filtering predicate.</param>
    /// <param name="behaviour">The found behaviour instance.</param>
    /// <typeparam name="T">The behaviour type.</typeparam>
    /// <returns>true if the behaviour instance was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryGetBehaviour<T>(this NetworkIdentity identity, Predicate<T> predicate, out T behaviour)
    {
        if (identity == null)
            throw new ArgumentNullException(nameof(identity));
        
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        if (identity.NetworkBehaviours?.Length < 1)
        {
            behaviour = default;
            return false;
        }

        for (var i = 0; i < identity.NetworkBehaviours.Length; i++)
        {
            if (identity.NetworkBehaviours[i] is T target
                && predicate(target))
            {
                behaviour = target;
                return true;
            }
        }
        
        behaviour = default;
        return false;
    }
}