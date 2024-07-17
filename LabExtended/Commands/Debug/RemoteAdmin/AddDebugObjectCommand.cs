using LabExtended.API;
using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;

using LabExtended.Core.Commands;

using LabExtended.Utilities;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.RemoteAdmin
{
    public class AddDebugObjectCommand : CommandInfo
    {
        public override string Command => "addra";
        public override string Description => "Adds a debug RA object to the panel.";

        public object OnCalled(ExPlayer sender, RemoteAdminObjectFlags flags, RemoteAdminIconType icons)
        {
            var num = Generator.Instance.GetInt32(0, 50);
            sender.RemoteAdmin.AddObject(new DebugRemoteAdminObject($"test_{num}", $"test_{num}", flags, icons) { CustomId = $"debug_ra_{num}" });
            return "Object added.";
        }
    }
}
