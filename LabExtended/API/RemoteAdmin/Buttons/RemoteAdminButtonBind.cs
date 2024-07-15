using LabExtended.API.RemoteAdmin.Interfaces;

namespace LabExtended.API.RemoteAdmin.Buttons
{
    public struct RemoteAdminButtonBind
    {
        public readonly IRemoteAdminObject Object;
        public readonly string Name;

        public RemoteAdminButtonBind(IRemoteAdminObject remoteAdminObject, string name)
        {
            Object = remoteAdminObject;
            Name = name;
        }
    }
}