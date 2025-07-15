using UnityEngine;

namespace LabExtended.Extensions;

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

    public static T FindComponent<T>(this GameObject gameObject)
        => TryFindComponent<T>(gameObject, out var component) ? component : default;

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

    public static bool TryFindComponent<T>(this RaycastHit hit, out T component)
    {
        if (hit.collider == null || hit.transform?.root?.gameObject == null)
        {
            component = default;
            return false;
        }

        return hit.transform.root.gameObject.TryFindComponent<T>(out component);
    }
}