using RelativePositioning;

using UnityEngine;

namespace LabExtended.Core.Networking.Synchronization.Position
{
    public class PositionData
    {
        public Vector3 Position { get; set; } = default;
        public RelativePosition RelativePosition { get; set; } = default;

        public bool IsReset { get; set; } = true;
        
        public ushort SyncH { get; set; } = 0;
        public ushort SyncV { get; set; } = 0;
    }
}