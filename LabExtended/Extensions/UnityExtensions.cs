using UnityEngine;

namespace LabExtended.Extensions;

/// <summary>
/// Extensions targeting the Unity Engine runtime.
/// </summary>
public static class UnityExtensions
{
    /// <summary>
    /// Gets a random position inside a bounded box.
    /// </summary>
    /// <param name="bounds">The target bounds.</param>
    /// <param name="randomY">Whether or not to randomize the Y axis.</param>
    /// <param name="transform">The transform to apply the rotation of.</param>
    /// <returns>The random point.</returns>
    public static Vector3 GetRandom(this Bounds bounds, bool randomY = true, Transform? transform = null)
    {
        var point = new Vector3(
            UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
            randomY ? UnityEngine.Random.Range(bounds.min.y, bounds.max.y) : bounds.center.y,
            UnityEngine.Random.Range(bounds.min.z, bounds.max.z));

        if (transform != null)
            point = transform.TransformPoint(point);

        return point;
    }

    /// <summary>
    /// Attempts to find a component in a game object, ignoring it's hierarchy.
    /// </summary>
    /// <param name="gameObject">The target game object.</param>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>The found component (or null).</returns>
    public static T FindComponent<T>(this GameObject gameObject)
        => TryFindComponent<T>(gameObject, out var component) ? component : default!;

    /// <summary>
    /// Attempts to find a component in a game object, ignoring it's hierarchy.
    /// </summary>
    /// <param name="gameObject">The target game object.</param>
    /// <param name="component">The found component.</param>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>true if the component was found</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool TryFindComponent<T>(this GameObject gameObject, out T component)
    {
        if (gameObject is null || !gameObject)
            throw new ArgumentNullException(nameof(gameObject));

        if ((component = gameObject.GetComponent<T>()) is null
            && (component = gameObject.GetComponentInChildren<T>()) is null
            && (component = gameObject.GetComponentInParent<T>()) is null)
            return false;

        return true;
    }

    /// <summary>
    /// Attempts to find a component in a raycast's collider.
    /// </summary>
    /// <param name="hit">The target raycast hit.</param>
    /// <param name="component">The found component.</param>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>true if the component was found</returns>
    public static bool TryFindComponent<T>(this RaycastHit hit, out T component)
    {
        if (hit.collider == null || hit.transform?.root?.gameObject == null)
        {
            component = default!;
            return false;
        }

        return hit.transform.root.gameObject.TryFindComponent(out component);
    }
}