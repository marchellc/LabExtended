using CommandSystem;

using LabExtended.API.CustomCommands.Hints.Image;
using LabExtended.API.CustomCommands.Hints.Refresh;
using LabExtended.API.CustomCommands.Hints.Show;

using LabExtended.Commands;

namespace LabExtended.API.CustomCommands.Hints;

[CommandHandler(typeof(GameConsoleCommandHandler))]
[CommandHandler(typeof(RemoteAdminCommandHandler))]
public class HintsCommand : VanillaParentCommandBase
{
    public override string Command { get; } = "hints";
    public override string Description { get; } = "Hints command implementation.";

    public override void LoadGeneratedCommands()
    {
        base.LoadGeneratedCommands();
        
        RegisterCommand(new ShowCommand());
        RegisterCommand(new ImageCommand());
        RegisterCommand(new RefreshCommand());
    }
}