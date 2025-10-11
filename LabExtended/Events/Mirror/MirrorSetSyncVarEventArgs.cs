using LabExtended.API;

using Mirror;

using System.Reflection;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called after a new value is applied to a network behaviour's sync variable.
    /// </summary>
    public class MirrorSetSyncVarEventArgs : MirrorIdentityEventArgs
    {
        private PropertyInfo? property;

        /// <summary>
        /// Gets the targeted network behaviour instance.
        /// </summary>
        public NetworkBehaviour Behaviour { get; }

        /// <summary>
        /// Gets the type of the variable that was set.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets the property that was set.
        /// </summary>
        public PropertyInfo Property
        {
            get
            {
                if (property == null)
                    MirrorMethods.TryGetPropertyName(Behaviour.GetType(), DirtyBit, out property);

                return property;
            }
        }

        /// <summary>
        /// Gets or sets the dirty bit flag of the variable that was set.
        /// </summary>
        public ulong DirtyBit { get; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public object? PreviousValue { get; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public object? NewValue { get; }

        public MirrorSetSyncVarEventArgs(NetworkBehaviour behaviour, Type type, ulong dirtyBit, object? previousValue, object? newValue) : base(behaviour.netIdentity)
        {
            Behaviour = behaviour;
            Type = type;
            DirtyBit = dirtyBit;
            PreviousValue = previousValue;
            NewValue = newValue;
        }
    }
}