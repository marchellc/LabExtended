using CommandSystem;

using LabExtended.API.CustomCommands.Despawn.LookAt;
using LabExtended.API.CustomCommands.Despawn.NetId;

using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Despawn
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    public class DespawnCommand : VanillaParentCommandBase
    {
        public override string Command => "despawn";
        public override string Description => "Commands used to despawn in-game objects.";

        public override void LoadGeneratedCommands()
        {
            base.LoadGeneratedCommands();

            RegisterCommand(new NetIdCommand());
            RegisterCommand(new LookAtCommand());
        }
    }
}