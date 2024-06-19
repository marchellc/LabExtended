using CommandSystem;

using LabExtended.API.Npcs;

using PlayerRoles;

using PluginAPI.Core;

namespace LabExtended.Commands.Npcs
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NpcSpawnCommand : ICommand
    {
        public string Command => "npcspawn";
        public string Description => "Spawns a new NPC player.";

        public string[] Aliases { get; } = new string[] { "npcs" };

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = $"Invalid usage!\nnpcspawn <npc role>";
                return false;
            }

            if (!Player.TryGet(sender, out var player))
            {
                response = "This command can only be executed in the Remote Admin.";
                return false;
            }

            if (!Enum.TryParse<RoleTypeId>(arguments.At(0), true, out var npcRole))
            {
                response = $"Invalid role type.";
                return false;
            }

            NpcHandler.Spawn(null, npcRole, null, null, player.Position, npc =>
            {
                sender.Respond($"NPC spawned with ID: {npc.Id}!\nUse the npccontrol (or npcc) command to control it!");
            });

            response = $"Spawning a new NPC (role: {npcRole})";
            return true;
        }
    }
}