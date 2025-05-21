using LabExtended.API;

using Mirror;

namespace LabExtended.Events.Mirror;

/// <summary>
/// Gets called before a new observer (player) is added to a behaviour's observers list.
/// </summary>
public class MirrorAddingObserverEventArgs : BooleanEventArgs
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
    /// Creates a new <see cref="MirrorAddingObserverEventArgs"/> instance.
    /// </summary>
    /// <param name="observer">The observing player.</param>
    /// <param name="target">The target identity.</param>
    public MirrorAddingObserverEventArgs(ExPlayer observer, NetworkIdentity target)
    {
        Observer = observer;
        Target = target;
    }
}