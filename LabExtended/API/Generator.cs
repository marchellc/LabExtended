using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Wrappers;

using MapGeneration;
using MapGeneration.Distributors;

namespace LabExtended.API
{
    public class Generator : NetworkWrapper<Scp079Generator>
    {
        public Generator(Scp079Generator baseValue) : base(baseValue) { }

        public bool IsReady => Base.ActivationReady;

        public TimeSpan TimeLeft => TimeSpan.FromSeconds(Base._syncTime);

        public RoomIdentifier Room => Base.Room;

        public bool IsLocked
        {
            get => !Base.IsUnlocked;
            set => Base.IsUnlocked = !value;
        }

        public bool IsOpen
        {
            get => Base.IsOpen;
            set => Base.IsOpen = value;
        }

        public bool IsActivating
        {
            get => Base.Activating;
            set => Base.Activating = value;
        }

        public bool IsEngaged
        {
            get => Base.Engaged;
            set => Base.Engaged = value;
        }

        public short RemainingTime
        {
            get => Base.Network_syncTime;
            set => Base.Network_syncTime = value;
        }

        public float UnlockCooldown
        {
            get => Base._unlockCooldownTime;
            set => Base._unlockCooldownTime = value;
        }

        public float DeniedCooldown
        {
            get => Base._deniedCooldownTime;
            set => Base._deniedCooldownTime = value;
        }

        public float ActivationTime
        {
            get => Base._totalActivationTime;
            set => Base._totalActivationTime = value;
        }

        public float DeactivationTime
        {
            get => Base._totalDeactivationTime;
            set => Base._totalDeactivationTime = value;
        }

        public float RemainingCooldown
        {
            get => Base._targetCooldown;
            set => Base._targetCooldown = value;
        }

        public KeycardPermissions RequiredPermissions
        {
            get => Base._requiredPermission;
            set => Base._requiredPermission = value;
        }

        public Scp079Generator.GeneratorFlags Flags
        {
            get => (Scp079Generator.GeneratorFlags)Base.Network_flags;
            set => Base.Network_flags = (byte)value;
        }

        public void Open()
            => IsOpen = true;

        public void Close()
            => IsOpen = false;

        public void ToggleDoors()
            => IsOpen = !IsOpen;

        public void Lock()
            => IsLocked = true;

        public void Unlock()
            => IsLocked = false;

        public void ToggleLock()
            => IsLocked = !IsLocked;

        public void PlayDenied()
            => Base.RpcDenied();

        public void Engage()
            => IsEngaged = true;
    }
}