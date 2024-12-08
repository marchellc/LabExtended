using UnityEngine;

namespace LabExtended.Utilities.Unity
{
    /// <summary>
    /// A collision handler to help with component management.
    /// </summary>
    public class CollisionHandler : MonoBehaviour
    {
        /// <summary>
        /// Gets called when one of the collision handlers collides with something.
        /// </summary>
        public static event Action<CollisionHandler, Collision> OnCollision;

        private Func<Collision, bool> _onCollision;

        private void OnCollisionEnter(Collision collision)
        {
            OnCollision?.Invoke(this, collision);

            if (_onCollision is null)
                return;

            if (_onCollision(collision))
                Destroy(this);
        }

        /// <summary>
        /// Adds or gets an existing instance of <see cref="CollisionHandler"/> on a specific component.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to get a <see cref="CollisionHandler"/> from.</param>
        /// <param name="onCollision">The collision handler.</param>
        /// <returns>The <see cref="CollisionHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CollisionHandler GetOrAdd(GameObject gameObject, Func<Collision, bool> onCollision = null)
        {
            if (gameObject is null)
                throw new ArgumentNullException(nameof(gameObject));

            if (gameObject.TryGetComponent<CollisionHandler>(out var collisionHandler))
                return collisionHandler;

            if (onCollision is null)
                throw new ArgumentNullException(nameof(onCollision));

            collisionHandler = gameObject.AddComponent<CollisionHandler>();
            collisionHandler._onCollision = onCollision;

            return collisionHandler;
        }

        /// <summary>
        /// Adds or gets an existing instance of <see cref="CollisionHandler"/> on a specific behaviour.
        /// </summary>
        /// <param name="monoBehaviour">The <see cref="MonoBehaviour"/> to get a <see cref="CollisionHandler"/> from.</param>
        /// <param name="onCollision">The collision handler.</param>
        /// <returns>The <see cref="CollisionHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CollisionHandler GetOrAdd(MonoBehaviour monoBehaviour, Func<Collision, bool> onCollision = null)
            => GetOrAdd(monoBehaviour.gameObject, onCollision);
    }
}