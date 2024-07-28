using Interactables.Interobjects;

using LabExtended.API.Collections.Locked;

namespace LabExtended.API
{
    public class Airlock : Wrapper<AirlockController>
    {
        internal static readonly LockedDictionary<AirlockController, Airlock> _wrappers = new LockedDictionary<AirlockController, Airlock>();

        public Airlock(AirlockController baseValue) : base(baseValue)
        {
            DoorA = ExMap.GetDoor(baseValue._doorA);
            DoorB = ExMap.GetDoor(baseValue._doorB);
        }

        public Door DoorA { get; }
        public Door DoorB { get; }

        public bool IsDisabled
        {
            get => Base.AirlockDisabled;
            set => Base.AirlockDisabled = value;
        }

        public bool AreDoorsLocked
        {
            get => Base._doorsLocked;
            set => Base._doorsLocked = value;
        }

        public void Toggle()
            => Base.ToggleAirlock();

        public void PlayAlarm()
            => Base.RpcAlarm();
    }
}