namespace LabExtended.Modules
{
    /// <summary>
    /// A class used for easier module & tick management.
    /// </summary>
    public class ModuleContainer
    {
        /// <summary>
        /// The module tied to this tick status.
        /// </summary>
        public Module Module { get; }

        /// <summary>
        /// The tick status tied to this module.
        /// </summary>
        public ModuleTickStatusInfo TickStatus { get; }

        /// <summary>
        /// Creates a new <see cref="ModuleContainer"/> instance.
        /// </summary>
        /// <param name="module">The module to contain.</param>
        public ModuleContainer(Module module)
        {
            Module = module;
            TickStatus = new ModuleTickStatusInfo(module.TickSettings.HasValue ? module.TickSettings.Value : new ModuleTickSettings(ModuleTickType.OnUpdate, null, null, null, null));
        }
    }
}