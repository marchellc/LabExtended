using Mirror;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before Mirror spawns a new network identity.
    /// </summary>
    public class MirrorSpawnedIdentityEventArgs : MirrorIdentityEventArgs
    {
        public MirrorSpawnedIdentityEventArgs(NetworkIdentity identity) : base(identity) 
        { 

        }
    }
}