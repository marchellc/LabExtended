namespace LabExtended.Modules
{
    /// <summary>
    /// A class that represents a custom module.
    /// </summary>
    public class Module
    {
        /// <summary>
        /// When overriden, retrieves this module's tick settings.
        /// </summary>
        public virtual ModuleTickSettings? TickSettings { get; }

        /// <summary>
        /// Gets the module's parent.
        /// </summary>
        public ModuleParent Parent { get; internal set; }

        /// <summary>
        /// Method called repeatedly as configured in <see cref="TickSettings"/>.
        /// </summary>
        public virtual void Tick() { }

        /// <summary>
        /// Method called when this module gets added to a <see cref="ModuleParent"/>.
        /// </summary>
        public virtual void Start() { }

        /// <summary>
        /// Method called when this module gets removed from a <see cref="ModuleParent"/>
        /// </summary>
        public virtual void Stop() { }
    }
}