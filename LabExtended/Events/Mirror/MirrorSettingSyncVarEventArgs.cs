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
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorSettingSyncVarEventArgs"/> class.
        /// </summary>
        internal static MirrorSettingSyncVarEventArgs Singleton { get; } = new();

        internal PropertyInfo? property;
        internal ulong dirtyBit;

        /// <summary>
        /// Gets the targeted network behaviour instance.
        /// </summary>
        public NetworkBehaviour Behaviour { get; internal set; }

        /// <summary>
        /// Gets the type of the variable being set.
        /// </summary>
        public Type Type { get; internal set; }

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
        public object? CurrentValue { get; internal set; }

        /// <summary>
        /// Gets or sets the new value of the property.
        /// </summary>
        public object? NewValue { get; set; }
    }
}