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
            Buttons[RemoteAdminButtonType.RequestAuth] = "Request AUTH";
            Buttons[RemoteAdminButtonType.RequestIp] = "Request IP";
            Buttons[RemoteAdminButtonType.Request] = "Request";
        }

        public override void OnEnabled()
        {
            base.OnEnabled();

            RemoteAdminButtons.BindButton(RemoteAdminButtonType.RequestAuth, this);
            RemoteAdminButtons.BindButton(RemoteAdminButtonType.Request, this);
            RemoteAdminButtons.BindButton(RemoteAdminButtonType.RequestIp, this);
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