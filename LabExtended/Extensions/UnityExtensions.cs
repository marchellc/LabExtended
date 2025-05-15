using UnityEngine;

namespace LabExtended.Extensions
{
    public static class UnityExtensions
    {
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
}