using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Enums;
using LabExtended.API.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.API
{
    /// <summary>
    /// Represents a checkpoint door.
    /// </summary>
    public class Checkpoint : Door, IWrapper<CheckpointDoor>
    {
        internal Checkpoint(CheckpointDoor baseValue, DoorType type) : base(baseValue, type)
        {
            Base = baseValue;
            Subdoors = baseValue.SubDoors.Select(Get);
        }

        /// <summary>
        /// The base of this wrapper.
        /// </summary>
        public new CheckpointDoor Base { get; }

        /// <summary>
        /// The checkpoint's doors.
        /// </summary>
        public IEnumerable<Door> Subdoors { get; }

        /// <summary>
        /// Gets or sets the checkpoint's current stage.
        /// </summary>
        public CheckpointDoor.CheckpointSequenceStage Stage
        {
            get => Base.CurrentSequence;
            set => Base.CurrentSequence = value;
        }

        /// <summary>
        /// Gets or sets the checkpoint's timer.
        /// </summary>
        public float MainTimer
        {
            get => Base.MainTimer;
            set => Base.MainTimer = value;
        }

        /// <summary>
        /// Gets or sets the checkpoint's wait time.
        /// </summary>
        public float WaitTime
        {
            get => Base.WaitTime;
            set => Base.WaitTime = value;
        }

        /// <summary>
        /// Gets or sets the checkpoint's warning time.
        /// </summary>
        public float WarningTime
        {
            get => Base.WarningTime;
            set => Base.WarningTime = value;
        }

        /// <inheritdoc cref="Door.Health"/>
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

        /// <inheritdoc cref="Door.MaxHealth"/>
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

        /// <inheritdoc cref="Door.IgnoredDamage"/>
        public new DoorDamageType IgnoredDamage
        {
            get => Subdoors.Aggregate(DoorDamageType.None, (current, door) => current | door.IgnoredDamage);
            set => Subdoors.ForEach(x => x.IgnoredDamage = value);
        }

        /// <summary>
        /// Opens the checkpoint doors.
        /// </summary>
        public void OpenDoors()
            => SetDoors(true);

        /// <summary>
        /// Closes the checkpoint doors.
        /// </summary>
        public void CloseDoors()
            => SetDoors(false);

        /// <summary>
        /// Toggles the checkpoint doors.
        /// </summary>
        public void ToggleDoors()
            => Base.ToggleAllDoors(!Base.NetworkTargetState);

        /// <summary>
        /// Sets the status of checkpoint doors.
        /// </summary>
        /// <param name="newState"></param>
        public void SetDoors(bool newState)
            => Base.ToggleAllDoors(newState);

        /// <inheritdoc cref="Door.Damage"/>
        public new bool Damage(float amount, DoorDamageType type = DoorDamageType.ServerCommand)
            => Base.ServerDamage(amount, type);

        /// <inheritdoc cref="Door.Destroy"/>
        public new bool Destroy(DoorDamageType type = DoorDamageType.ServerCommand)
            => Base.ServerDamage(float.MaxValue, type);
    }
}