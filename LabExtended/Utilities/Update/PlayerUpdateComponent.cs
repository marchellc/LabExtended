using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Utilities.Update
{
    /// <summary>
    /// Used to provide an update delegate on Unity's player loop.
    /// </summary>
    public class PlayerUpdateComponent : MonoBehaviour
    {
        /// <summary>
        /// Occurs when the object is updated, allowing subscribers to respond to changes.
        /// </summary>
        /// <remarks>Subscribe to this event to be notified whenever an update occurs. The event handler
        /// does not receive any event data.</remarks>
        public event Action? OnUpdate;

        /// <summary>
        /// Occurs after the main update cycle, allowing subscribers to perform actions that should run late in the
        /// frame.
        /// </summary>
        /// <remarks>Use this event to execute logic that must happen after all standard update operations
        /// have completed. This is commonly used for tasks such as cleanup, post-processing, or deferred actions that
        /// depend on the results of earlier updates. Subscribers should ensure their handlers are efficient to avoid
        /// impacting overall frame performance.</remarks>
        public event Action? OnLateUpdate;

        /// <summary>
        /// Occurs on each fixed update cycle, allowing subscribers to perform actions at a consistent interval.
        /// </summary>
        /// <remarks>Subscribe to this event to execute logic that should run at a fixed timestep, such as
        /// physics calculations or time-based updates. The timing and frequency of the fixed update cycle depend on the
        /// host environment.</remarks>
        public event Action? OnFixedUpdate;

        /// <summary>
        /// Destroys the associated game object, removing it from the scene and freeing its resources.
        /// </summary>
        /// <remarks>If the game object is already null, this method has no effect. After calling this
        /// method, references to the destroyed game object will be invalid.</remarks>
        public void Destroy()
        {
            if (gameObject != null)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Removes all update event handlers, resetting the update, late update, and fixed update delegates to null.
        /// </summary>
        public void Clear()
        {
            OnUpdate = null;
            OnLateUpdate = null;
            OnFixedUpdate = null;
        }

        void Update()
        {
            OnUpdate?.InvokeSafe();
        }

        void LateUpdate()
        {
            OnLateUpdate?.InvokeSafe();
        }

        void FixedUpdate()
        {
            OnFixedUpdate?.InvokeSafe();
        }

        void OnDestroy()
            => Clear();

        /// <summary>
        /// Creates a new PlayerUpdateComponent attached to a persistent GameObject in the scene.
        /// </summary>
        /// <returns>A PlayerUpdateComponent instance attached to a GameObject that will not be destroyed when loading a new
        /// scene.</returns>
        public static PlayerUpdateComponent Create()
        {
            var gameObject = new GameObject($"playerUpdateComponent.{DateTime.Now.Ticks}");

            DontDestroyOnLoad(gameObject);
            return gameObject.AddComponent<PlayerUpdateComponent>();
        }
    }
}