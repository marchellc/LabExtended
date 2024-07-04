using LabExtended.API;
using LabExtended.Core.Commands;
using LabExtended.Utilities.Debug;

namespace LabExtended.Commands.Debug.CustomItems
{
    public class SpawnDebugItemCommand : CommandInfo
    {
        public override string Command => "spawnitem";
        public override string Description => "Spawns the debug custom item.";

        public object OnCalled(ExPlayer sender)
        {
            DebugCustomItem.RegisterItem();
            DebugCustomItem.Spawn<DebugCustomItem>(sender.Position, sender.Rotation, null, sender);

            return "Spawned the debug custom item.";
        }
    }
}