using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror spawns a new network identity.
    /// </summary>
    public class MirrorSpawningIdentityEventArgs : MirrorIdentityBooleanEventArgs
    {
        public MirrorSpawningIdentityEventArgs(NetworkIdentity identity) : base(identity)
        {

        }
    }
}