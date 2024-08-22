using LabExtended.API.CustomItems.Info;

namespace LabExtended.API.CustomItems.Usables
{
    public class CustomUsableInfo : CustomItemInfo
    {
        public float UseTime { get; set; } = 0f;
        public float CooldownTime { get; set; } = 0f;

        public CustomUsableInfo(Type type, string id, string name, string description, ItemType inventoryType, CustomItemFlags itemFlags, float useTime, float cooldownTime, CustomItemPickupInfo pickupInfo) : base(type, id, name, description, inventoryType, itemFlags, pickupInfo)
        {
            UseTime = useTime;
            CooldownTime = cooldownTime;
        }
    }
}