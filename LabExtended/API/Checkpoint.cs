using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Enums;
using LabExtended.API.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.API
{
    public class Checkpoint : Door, IWrapper<CheckpointDoor>
    {
        public Checkpoint(CheckpointDoor baseValue, DoorType type) : base(baseValue, type)
        {
            Base = baseValue;
            Subdoors = baseValue.SubDoors.Select(ExMap.GetDoor);
        }

        public new CheckpointDoor Base { get; }

        public IEnumerable<Door> Subdoors { get; }

        public CheckpointDoor.CheckpointSequenceStage Stage
        {
            get => Base._currentSequence;
            set => Base._currentSequence = value;
        }

        public float MainTimer
        {
            get => Base._mainTimer;
            set => Base._mainTimer = value;
        }

        public float WaitTime
        {
            get => Base._waitTime;
            set => Base._waitTime = value;
        }

        public float WarningTime
        {
            get => Base._warningTime;
            set => Base._warningTime = value;
        }

        public new float Health
        {
            get => Base.GetHealthPercent();
            set
            {
                var health = value / Subdoors.Count();

                foreach (var door in Subdoors)
                    door.Health = health;
            }
        }

        public new float MaxHealth
        {
            get => Subdoors.Sum(x => x.MaxHealth);
            set
            {
                var health = value / Subdoors.Count();

                foreach (var door in Subdoors)
                    door.MaxHealth = health;
            }
        }

        public new DoorDamageType IgnoredDamage
        {
            get => Subdoors.Aggregate(DoorDamageType.None, (current, door) => current | door.IgnoredDamage);
            set => Subdoors.ForEach(x => x.IgnoredDamage = value);
        }

        public void OpenDoors()
            => SetDoors(true);

        public void CloseDoors()
            => SetDoors(false);

        public void ToggleDoors()
            => Base.ToggleAllDoors(!Base.NetworkTargetState);

        public void SetDoors(bool newState)
            => Base.ToggleAllDoors(newState);

        public new bool Damage(float amount, DoorDamageType type = DoorDamageType.ServerCommand)
            => Base.ServerDamage(amount, type);

        public new bool Destroy(DoorDamageType type = DoorDamageType.ServerCommand)
            => Base.ServerDamage(float.MaxValue, type);
    }
}