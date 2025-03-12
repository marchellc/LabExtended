using Interactables.Interobjects.DoorUtils;

using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;

using MapGeneration;
using MapGeneration.Distributors;

namespace LabExtended.API
{
    /// <summary>
    /// Represents an in-game SCP-079 generator.
    /// </summary>
    public class Generator : NetworkWrapper<Scp079Generator>
    {
        /// <summary>
        /// Gets all spawned generators.
        /// </summary>
        public static Dictionary<Scp079Generator, Generator> Lookup { get; } = new();

        /// <summary>
        /// Tries to find a wrapper by it's base object.
        /// </summary>
        /// <param name="generator">The base object.</param>
        /// <param name="wrapper">The wrapper instance.</param>
        /// <returns>The wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(Scp079Generator generator, out Generator? wrapper)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));
            
            return Lookup.TryGetValue(generator, out wrapper);
        }

        /// <summary>
        /// Gets a wrapper by it's base object.
        /// </summary>
        /// <param name="generator">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Generator Get(Scp079Generator generator)
        {
            if (generator is null)
                throw new ArgumentNullException(nameof(generator));

            if (!Lookup.TryGetValue(generator, out var wrapper))
                throw new KeyNotFoundException($"SCP-079 Generator {generator.netId} could not be found");
            
            return wrapper;
        }
        
        internal Generator(Scp079Generator baseValue) : base(baseValue) { }

        /// <summary>
        /// Whether or not this generator is ready for activation.
        /// </summary>
        public bool IsReady => Base.ActivationReady;

        /// <summary>
        /// Time left till activation.
        /// </summary>
        public TimeSpan TimeLeft => TimeSpan.FromSeconds(Base._syncTime);

        /// <summary>
        /// The room the generator is located in.
        /// </summary>
        public RoomIdentifier Room => Base.Room;

        /// <summary>
        /// Whether or not the generator's doors are locked.
        /// </summary>
        public bool IsLocked
        {
            get => !Base.IsUnlocked;
            set => Base.IsUnlocked = !value;
        }

        /// <summary>
        /// Whether or not the generator's doors are opened.
        /// </summary>
        public bool IsOpen
        {
            get => Base.IsOpen;
            set => Base.IsOpen = value;
        }

        /// <summary>
        /// Whether or not the generator is activating.
        /// </summary>
        public bool IsActivating
        {
            get => Base.Activating;
            set => Base.Activating = value;
        }

        /// <summary>
        /// Whether or not the generator is engaged.
        /// </summary>
        public bool IsEngaged
        {
            get => Base.Engaged;
            set => Base.Engaged = value;
        }

        /// <summary>
        /// Remaining time to activation.
        /// </summary>
        public short RemainingTime
        {
            get => Base.Network_syncTime;
            set => Base.Network_syncTime = value;
        }

        /// <summary>
        /// Unlock cooldown time.
        /// </summary>
        public float UnlockCooldown
        {
            get => Base._unlockCooldownTime;
            set => Base._unlockCooldownTime = value;
        }

        /// <summary>
        /// Cooldown after a failed door interaction.
        /// </summary>
        public float DeniedCooldown
        {
            get => Base._deniedCooldownTime;
            set => Base._deniedCooldownTime = value;
        }

        /// <summary>
        /// How long it takes to activate this generator.
        /// </summary>
        public float ActivationTime
        {
            get => Base._totalActivationTime;
            set => Base._totalActivationTime = value;
        }

        /// <summary>
        /// How long it takes to deactivate this generator.
        /// </summary>
        public float DeactivationTime
        {
            get => Base._totalDeactivationTime;
            set => Base._totalDeactivationTime = value;
        }

        /// <summary>
        /// Remaining generator cooldown.
        /// </summary>
        public float RemainingCooldown
        {
            get => Base._targetCooldown;
            set => Base._targetCooldown = value;
        }

        /// <summary>
        /// Gets or sets the permissions required to open this generator.
        /// </summary>
        public KeycardPermissions RequiredPermissions
        {
            get => Base._requiredPermission;
            set => Base._requiredPermission = value;
        }

        /// <summary>
        /// Gets or sets the generator's network flags.
        /// </summary>
        public Scp079Generator.GeneratorFlags Flags
        {
            get => (Scp079Generator.GeneratorFlags)Base.Network_flags;
            set => Base.Network_flags = (byte)value;
        }

        /// <summary>
        /// Opens the doors.
        /// </summary>
        public void Open()
            => IsOpen = true;

        /// <summary>
        /// Closes the doors.
        /// </summary>
        public void Close()
            => IsOpen = false;

        /// <summary>
        /// Toggles the doors open state.
        /// </summary>
        public void ToggleDoors()
            => IsOpen = !IsOpen;

        /// <summary>
        /// Locks the doors.
        /// </summary>
        public void Lock()
            => IsLocked = true;

        /// <summary>
        /// Unlocks the doors.
        /// </summary>
        public void Unlock()
            => IsLocked = false;

        /// <summary>
        /// Toggles the door's lock.
        /// </summary>
        public void ToggleLock()
            => IsLocked = !IsLocked;

        /// <summary>
        /// Plays the denied sound effect.
        /// </summary>
        public void PlayDenied()
            => Base.RpcDenied();

        /// <summary>
        /// Engages this generator.
        /// </summary>
        public void Engage()
            => IsEngaged = true;

        private static void OnGeneratorSpawned(Scp079Generator generator)
            => Lookup.Add(generator, new(generator));

        private static void OnGeneratorDestroyed(Scp079Generator generator)
            => Lookup.Remove(generator);

        [LoaderInitialize(1)]
        private static void OnInit()
        {
            Scp079Generator.OnAdded += OnGeneratorSpawned;
            Scp079Generator.OnRemoved += OnGeneratorDestroyed;

            InternalEvents.OnRoundWaiting += Lookup.Clear;
        }
    }
}