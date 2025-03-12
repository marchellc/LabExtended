using Interactables.Interobjects;

using LabExtended.API.Wrappers;
using LabExtended.Attributes;
using LabExtended.Events;

namespace LabExtended.API
{
    /// <summary>
    /// Represents an in-game airlock door.
    /// </summary>
    public class Airlock : NetworkWrapper<AirlockController>
    {
        /// <summary>
        /// Gets all spawned airlocks.
        /// </summary>
        public static Dictionary<AirlockController, Airlock> Lookup { get; } = new();

        /// <summary>
        /// Tries to get a wrapper from its base object.
        /// </summary>
        /// <param name="controller">The base object.</param>
        /// <param name="airlock">The found wrapper instance.</param>
        /// <returns>true if the wrapper instance was found.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool TryGet(AirlockController controller, out Airlock airlock)
        {
            if (controller is null)
                throw new ArgumentNullException(nameof(controller));
            
            return Lookup.TryGetValue(controller, out airlock);
        }

        /// <summary>
        /// Gets a wrapper from its base object.
        /// </summary>
        /// <param name="controller">The base object.</param>
        /// <returns>The found wrapper instance.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public static Airlock Get(AirlockController controller)
        {
            if (controller is null)
                throw new ArgumentNullException(nameof(controller));

            if (!Lookup.TryGetValue(controller, out var airlock))
                throw new KeyNotFoundException($"Could not find AirlockController {controller.netId}");
            
            return airlock;
        }
        
        internal Airlock(AirlockController baseValue) : base(baseValue)
        {
            DoorA = Door.Get(baseValue._doorA);
            DoorB = Door.Get(baseValue._doorB);
        }

        /// <summary>
        /// Gets the airlock's primary door.
        /// </summary>
        public Door DoorA { get; }
        
        /// <summary>
        /// Gets the airlock's secondary door.
        /// </summary>
        public Door DoorB { get; }

        /// <summary>
        /// Whether or not the airlock is disabled.
        /// </summary>
        public bool IsDisabled
        {
            get => Base.AirlockDisabled;
            set => Base.AirlockDisabled = value;
        }

        /// <summary>
        /// Whether or not the airlock doors are locked.
        /// </summary>
        public bool AreDoorsLocked
        {
            get => Base._doorsLocked;
            set => Base._doorsLocked = value;
        }

        /// <summary>
        /// Toggles the airlock's doors.
        /// </summary>
        public void Toggle()
            => Base.ToggleAirlock();

        /// <summary>
        /// Plays the airlock's alarm sound.
        /// </summary>
        public void PlayAlarm()
            => Base.RpcAlarm();

        private static void OnWaiting()
        {
            foreach (var airlock in UnityEngine.Object.FindObjectsOfType<AirlockController>())
                Lookup.Add(airlock, new(airlock));
        }
        
        [LoaderInitialize(1)]
        private static void OnInit()
        {
            InternalEvents.OnRoundWaiting += OnWaiting;
            InternalEvents.OnRoundRestart += Lookup.Clear;
        }
    }
}