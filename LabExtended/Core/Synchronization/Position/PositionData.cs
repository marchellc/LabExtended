using RelativePositioning;

namespace LabExtended.Core.Synchronization.Position
{
    public class PositionData
    {
        public RelativePosition Position = default;

        public ushort SyncH = 0;
        public ushort SyncV = 0;
    }
}