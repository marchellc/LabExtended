using CommandSystem;

using LabExtended.API.CustomCommands.Spawn.Prefab;
using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Spawn
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class SpawnCommand : VanillaParentCommandBase
    {
        public override string Command => "spawn";
        public override string Description => "Commands used to spawn in-game objects.";

        public override void LoadGeneratedCommands()
        {
            base.LoadGeneratedCommands();

            RegisterCommand(new PrefabCommand());
        }
    }
}