using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin;
using LabExtended.API.RemoteAdmin.Enums;
using LabExtended.API.RemoteAdmin.Objects;

namespace LabExtended.Utilities.Debug
{
    public class DebugRemoteAdminObject : StaticRemoteAdminObject
    {
        public DebugRemoteAdminObject(string name, string response, RemoteAdminObjectFlags flags = RemoteAdminObjectFlags.ShowToNorthwoodStaff, RemoteAdminIconType icons = RemoteAdminIconType.None) : base(name, response, flags, icons)
        {

        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            RemoteAdminButtons.BindButton(RemoteAdminButtonType.RequestAuth, this, Name + " 1");
            RemoteAdminButtons.BindButton(RemoteAdminButtonType.Request, this, Name + " 2");
            RemoteAdminButtons.BindButton(RemoteAdminButtonType.RequestIp, this, Name + " 3");
        }

        public override void OnDisabled()
        {
            base.OnDisabled();

            RemoteAdminButtons.UnbindButton(RemoteAdminButtonType.RequestAuth, this);
            RemoteAdminButtons.UnbindButton(RemoteAdminButtonType.Request, this);
            RemoteAdminButtons.UnbindButton(RemoteAdminButtonType.RequestIp, this);
        }
    }
}