using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;

using Mirror;

namespace LabExtended.API.CustomCommands.Despawn.NetId
{
    public class NetIdCommand : CustomCommand
    {
        public override string Command => "netid";
        public override string Description => "Despawns an object via it's Network ID";

        public override ArgumentDefinition[] BuildArgs()
        {
            return GetArg<uint>("NetID", "The network ID of the object.");
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);

            var netId = args.GetUInt("NetID");

            if (!NetworkServer.spawned.TryGetValue(netId, out var identity))
            {
                ctx.RespondFail($"Failed to find an active NetworkIdentity component of ID {netId}");
                return;
            }

            var name = identity.gameObject.name;

            NetworkServer.Destroy(identity.gameObject);

            ctx.RespondOk($"Object '{identity.gameObject.name}' destroyed.");
        }
    }
}
