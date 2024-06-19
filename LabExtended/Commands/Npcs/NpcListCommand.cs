using CommandSystem;

using LabExtended.API.Npcs;

namespace LabExtended.Commands.Npcs
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class NpcListCommand : ICommand
    {
        public string Command => "npclist";
        public string Description => "Shows a list of active NPCs.";

        public string[] Aliases { get; } = new string[] { "npcl" };

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (NpcHandler.Count < 1)
            {
                response = "There aren't any active NPCs.";
                return true;
            }

            response = $"Active NPCs ({NpcHandler.Count}):\n";

            foreach (var npc in NpcHandler.Npcs)
            {
                if (npc.IsSpawned)
                    response += $"[SPAWNED] {npc.Id} ({npc.Player.Role})";
                else
                    response += $"[DESPAWNED] {npc.Id}";
            }

            return true;
        }
    }
}
