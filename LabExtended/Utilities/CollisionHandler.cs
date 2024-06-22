using UnityEngine;

namespace LabExtended.Utilities
{
    /// <summary>
    /// A collision handler to help with component management.
    /// </summary>
    public class CollisionHandler : MonoBehaviour
    {
        /// <summary>
        /// Gets called when one of the collision handlers collides with something.
        /// </summary>
        public static event Action<CollisionHandler, Collision> OnTrigger;

        private Func<Collision, bool> _onTrigger;

        private void OnTriggerEnter(Collision collision)
        {
            OnTrigger?.Invoke(this, collision);

            if (_onTrigger is null)
                return;

            if (_onTrigger(collision))
                Destroy(this);
        }

        /// <summary>
        /// Adds or gets an existing instance of <see cref="CollisionHandler"/> on a specific component.
        /// </summary>
        /// <param name="gameObject">The <see cref="GameObject"/> to get a <see cref="CollisionHandler"/> from.</param>
        /// <param name="onTrigger">The collision handler.</param>
        /// <returns>The <see cref="CollisionHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CollisionHandler GetOrAdd(GameObject gameObject, Func<Collision, bool> onTrigger = null)
        {
            if (gameObject is null)
                throw new ArgumentNullException(nameof(gameObject));

            if (gameObject.TryGetComponent<CollisionHandler>(out var collisionHandler))
                return collisionHandler;

            if (onTrigger is null)
                throw new ArgumentNullException(nameof(onTrigger));

            collisionHandler = gameObject.AddComponent<CollisionHandler>();
            collisionHandler._onTrigger = onTrigger;

            return collisionHandler;
        }

        /// <summary>
        /// Adds or gets an existing instance of <see cref="CollisionHandler"/> on a specific behaviour.
        /// </summary>
        /// <param name="monoBehaviour">The <see cref="MonoBehaviour"/> to get a <see cref="CollisionHandler"/> from.</param>
        /// <param name="onCollision">The collision handler.</param>
        /// <returns>The <see cref="CollisionHandler"/> instance if found, otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static CollisionHandler GetOrAdd(MonoBehaviour monoBehaviour, Func<Collision, bool> onCollision)
            => GetOrAdd(monoBehaviour.gameObject);
    }
}