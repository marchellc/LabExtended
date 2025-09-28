using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called after a new observer is added to a <see cref="NetworkIdentity"/>'s observers list.
    /// </summary>
    public class MirrorAddedObserverEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the new observer.
        /// </summary>
        public ExPlayer Observer { get; }

        /// <summary>
        /// Gets the target identity.
        /// </summary>
        public NetworkIdentity Target { get; }

        /// <summary>
        /// Creates a new <see cref="MirrorAddedObserverEventArgs"/> instance.
        /// </summary>
        /// <param name="observer">The observing player.</param>
        /// <param name="target">The target identity.</param>
        public MirrorAddedObserverEventArgs(ExPlayer observer, NetworkIdentity target)
        {
            Observer = observer;
            Target = target;
        }
    }
}
