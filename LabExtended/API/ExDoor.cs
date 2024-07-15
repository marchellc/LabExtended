using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Collections.Locked;
using LabExtended.Utilities;

namespace LabExtended.API
{
    public class ExDoor : Wrapper<DoorVariant>
    {
        private static readonly LockedDictionary<DoorVariant, ExDoor> _doors = new LockedDictionary<DoorVariant, ExDoor>();

        public static IEnumerable<ExDoor> Doors => _doors.Values;

        public static int Count => _doors.Count;

        public ExDoor(DoorVariant baseValue) : base(baseValue)
        {

        }
    }
}