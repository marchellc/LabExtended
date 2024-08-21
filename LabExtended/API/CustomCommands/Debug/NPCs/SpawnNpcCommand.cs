using LabExtended.API.Npcs;
using LabExtended.Commands;
using LabExtended.Commands.Arguments;
using LabExtended.Commands.Interfaces;
using PlayerRoles;

namespace LabExtended.API.CustomCommands.Debug.Hints
{
    public class SpawnNpcCommand : CustomCommand
    {
        public override string Command => "spawnnpc";
        public override string Description => "Spawns NPC";

        public override ArgumentDefinition[] BuildArgs() {
            return ArgumentBuilder.Get(x => {
                x.WithArg<bool>("NpcSwitches", "Spawn with Default NPC Switches");
            });
        }

        public override void OnCommand(ExPlayer sender, ICommandContext ctx, ArgumentCollection args)
        {
            base.OnCommand(sender, ctx, args);
            var npcSwitches = args.Get<bool>("NpcSwitches");

            NpcHandler.Spawn(role: RoleTypeId.Tutorial, position: sender.Position, callback: npc => {
                npc.Player.Name = $"Test Subject {npc.Id}";

                if (!npcSwitches) {
                    npc.Player.Switches.Copy(ExPlayer.PlayerSwitches);
                }

                sender.SendRemoteAdminMessage($"Spawned NPC with ID: {npc.Id}, PlayerID: {npc.Player.PlayerId}");
            });

            ctx.RespondOk("Spawning ...");
        }
    }
}