using LabExtended.API;

using Mirror;

using System.Reflection;

namespace LabExtended.Events.Mirror
{
    /// <summary>
    /// Gets called before a new value is applied to a network behaviour's sync variable.
    /// </summary>
    public class MirrorSettingSyncVarEventArgs : MirrorIdentityBooleanEventArgs
    {
        private PropertyInfo? property;
        private ulong dirtyBit;

        /// <summary>
        /// Gets the targeted network behaviour instance.
        /// </summary>
        public NetworkBehaviour Behaviour { get; }

        /// <summary>
        /// Gets the type of the variable being set.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Gets 
        /// </summary>
        public PropertyInfo Property
        {
            get
            {
                if (property == null)
                    MirrorMethods.TryGetPropertyName(Behaviour.GetType(), dirtyBit, out property);

                return property;
            }
        }

        /// <summary>
        /// Gets or sets the dirty bit flag of the variable being set.
        /// </summary>
        public ulong DirtyBit { get; set; }

        /// <summary>
        /// Gets the current value of the property.
        /// </summary>
        public object? CurrentValue { get; }

        /// <summary>
        /// Gets or sets the new value of the property.
        /// </summary>
        public object? NewValue { get; set; }

        public MirrorSettingSyncVarEventArgs(NetworkBehaviour behaviour, Type type, ulong dirtyBit, object? currentValue, object? newValue) : base(behaviour.netIdentity)
        {
            Behaviour = behaviour;
            Type = type;
            DirtyBit = this.dirtyBit = dirtyBit;
            CurrentValue = currentValue;
            NewValue = newValue;
        }
    }
}