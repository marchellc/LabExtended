using CommandSystem;

using LabExtended.API.CustomCommands.Image.Destroy;
using LabExtended.API.CustomCommands.Image.Loop;
using LabExtended.API.CustomCommands.Image.Pause;
using LabExtended.API.CustomCommands.Image.Play;
using LabExtended.API.CustomCommands.Image.Spawn;
using LabExtended.API.CustomCommands.Image.Stop;

using LabExtended.API.Toys.Primitives;

using LabExtended.Attributes;
using LabExtended.Commands;
using LabExtended.Events;

using Utils.NonAllocLINQ;

namespace LabExtended.API.CustomCommands.Image;

[CommandHandler(typeof(RemoteAdminCommandHandler))]
[CommandHandler(typeof(GameConsoleCommandHandler))]
public class ImageCommand : VanillaParentCommandBase
{
    #region Static Methods
    private static int idClock = 0;
    
    public static int NewId => idClock++;
    
    public static Dictionary<int, PrimitiveDynamicImage> SpawnedImages { get; } =
        new Dictionary<int, PrimitiveDynamicImage>();

    private static void OnRoundRestart()
    {
        SpawnedImages.ForEachValue(x => x.Dispose());
        SpawnedImages.Clear();
        
        idClock = 0;
    }

    [LoaderInitialize(1)]
    private static void OnInit()
        => ExRoundEvents.Restarting += OnRoundRestart;
    #endregion

    public override string Command { get; } = "image";
    public override string Description { get; } = "Commands for spawning images via primitive objects.";

    public override void LoadGeneratedCommands()
    {
        base.LoadGeneratedCommands();
        
        RegisterCommand(new DestroyCommand());
        RegisterCommand(new LoopCommand());
        RegisterCommand(new PauseCommand());
        RegisterCommand(new PlayCommand());
        RegisterCommand(new SpawnCommand());
        RegisterCommand(new StopCommand());
        
        RegisterCommand(new Parent.ParentCommand());
    }
}