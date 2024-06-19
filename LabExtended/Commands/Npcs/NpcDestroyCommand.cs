using CommandSystem;

using LabExtended.API.Npcs;

namespace LabExtended.Commands.Npcs
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class NpcDestroyCommand : ICommand
    {
        public string Command => "npcdestroy";
        public string Description => "Destroys an NPC.";

        public string[] Aliases { get; } = new string[] { "npcd" };

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count != 1)
            {
                response = "Invalid usage!\nnpcdestroy <npc ID>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out var npcId))
            {
                response = "Invalid NPC ID!";
                return false;
            }

            if (!NpcHandler.TryGetById(npcId, out var npcHandler))
            {
                response = "Unknown NPC ID!";
                return false;
            }

            npcHandler.Destroy();

            response = $"NPC {npcHandler.Id} destroyed!";
            return true;
        }
    }
}