using LabExtended.API.Enums;
using LabExtended.API.RemoteAdmin.Enums;

using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;

using LabExtended.Utilities;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.RemoteAdmin
{
    public class AddDebugObjectCommand : CustomCommand
    {
        public override string Command => "addra";
        public override string Description => "Adds a debug RA object to the panel.";

        public override ArgumentDefinition[] Arguments { get; } = new ArgumentDefinition[]
        {
            ArgumentDefinition.FromType<RemoteAdminObjectFlags>("flags", "Flags of the object."),
            ArgumentDefinition.FromType<RemoteAdminIconType>("icons", "Icons that will be shown in the object list.", ArgumentFlags.Optional, RemoteAdminIconType.None)
        };

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var num = Generator.Instance.GetInt32(0, 50);

            var flags = args.Get<RemoteAdminObjectFlags>("flags");
            var icons = args.Get<RemoteAdminIconType>("icons");

            sender.RemoteAdmin.AddObject(new DebugRemoteAdminObject($"test_{num}", $"test_{num}", flags, icons) { CustomId = $"debug_ra_{num}" });

            ctx.RespondOk("Object added!");
        }
    }
}
