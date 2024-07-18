using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.API.CustomCommands.Debug.CustomItems
{
    public class SpawnDebugItemCommand : CommandInfo
    {
        public override string Command => "spawnitem";
        public override string Description => "Spawns the debug custom item.";

        public object OnCalled(ExPlayer sender)
        {
            DebugCustomItem.RegisterItem();
            API.CustomItems.CustomItem.Spawn<DebugCustomItem>(sender.Position, sender.Rotation, null, sender);

            return "Spawned the debug custom item.";
        }
    }
}