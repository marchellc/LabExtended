using UnityEngine;

namespace LabExtended.CustomItems
{
    public struct CustomItemPickupInfo
    {
        public Vector3 Scale { get; }

        public ItemType Type { get; }

        public CustomItemPickupInfo(Vector3 scale, ItemType type)
        {
            Scale = scale;
            Type = type;
        }
    }
}