using LabExtended.Commands;
using LabExtended.Commands.Arguments;

using LabExtended.Core.Commands.Interfaces;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Debug.API
{
    public class CameraLookCommand : CustomCommand
    {
        public override string Command => "camlook";
        public override string Description => "Forces a camera to look at you.";

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var room = sender.Room;

            if (room is null)
            {
                ctx.RespondFail("You are not in a valid room.");
                return;
            }

            sender.SendRemoteAdminMessage($"Room: {room.Name}, Position: {sender.Position}");

            var camera = ExMap.Cameras.OrderBy(x => Vector3.Distance(x.Position, sender.Position)).First();
            var distance = Vector3.Distance(camera.Position, sender.Position);

            sender.SendRemoteAdminMessage($"Camera: {camera.Name} {camera.Id} ({camera.RoomName}) [{distance}]");

            camera.LookAt(sender);

            ctx.RespondOk("Forced a camera to look at you.");
        }
    }
}