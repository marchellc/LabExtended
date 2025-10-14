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
        /// <summary>
        /// Gets the singleton instance of the <see cref="MirrorSetSyncVarEventArgs"/> class.
        /// </summary>
        internal static MirrorSetSyncVarEventArgs Singleton { get; } = new();

        internal PropertyInfo? property;

        /// <summary>
        /// Gets the targeted network behaviour instance.
        /// </summary>
        public NetworkBehaviour Behaviour { get; internal set; }

        /// <summary>
        /// Gets the type of the variable that was set.
        /// </summary>
        public Type Type { get; internal set; }

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
        public ulong DirtyBit { get; internal set; }

        /// <summary>
        /// Gets the previous value of the property.
        /// </summary>
        public object? PreviousValue { get; internal set; }

        /// <summary>
        /// Gets the new value of the property.
        /// </summary>
        public object? NewValue { get; internal set; }
    }
}