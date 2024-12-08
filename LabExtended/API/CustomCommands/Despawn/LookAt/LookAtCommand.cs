using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

using Mirror;

using UnityEngine;

namespace LabExtended.API.CustomCommands.Despawn.LookAt
{
    public class LookAtCommand : CustomCommand
    {
        public override string Command => "lookat";
        public override string Description => "Destroys the object you're currently looking at.";

        public override ArgumentDefinition[] BuildArgs()
        {
            return GetOptionalArg<float>("Distance", "Maximum destroy distance.", 100f);
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var distance = args.Get<float>("Distance");

            if (Physics.Raycast(sender.Position, sender.CameraTransform.forward, out var hit, distance, -1, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider is null)
                {
                    ctx.RespondFail("Hit collider is null.");
                    return;
                }

                if (hit.collider.gameObject is null)
                {
                    ctx.RespondFail("Hit collider object is null.");
                    return;
                }

                if (!hit.collider.gameObject.TryFindComponent<NetworkIdentity>(out var networkIdentity))
                {
                    ctx.RespondFail($"This object does not have a NetworkIdentity ({hit.collider.gameObject.name})");
                    return;
                }

                if (ExPlayer.TryGet(hit.collider, out var player))
                {
                    if (player == sender)
                    {
                        ctx.RespondFail("Hit yourself.");
                        return;
                    }

                    player.HasGodMode = false;
                    player.Kill();

                    ctx.RespondOk($"Killed {player.Name}");
                    return;
                }

                ctx.RespondOk($"Destroyed object {networkIdentity.name} ({networkIdentity.netId})");

                NetworkServer.Destroy(networkIdentity.gameObject);
            }
            else
            {
                ctx.RespondFail("Failed to hit anything.");
            }
        }
    }
}
