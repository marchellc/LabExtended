using LabExtended.API.RemoteAdmin;
using LabExtended.API;

using System.Text;

namespace LabExtended.Utilities
{
    public class DebugRemoteAdminObject : RemoteAdminObject
    {
        public DebugRemoteAdminObject() : base("Debug Object", "debugObject") { }

        public override void OnRequest(ExPlayer player, RemoteAdminPlayerRequestType remoteAdminPlayerRequestType, StringBuilder stringBuilder)
        {
            base.OnRequest(player, remoteAdminPlayerRequestType, stringBuilder);
            stringBuilder.Append($"[DEBUG OBJECT] {remoteAdminPlayerRequestType} Requested by: {player.Name} ({player.UserId})");
        }
    }
}