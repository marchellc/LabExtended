namespace LabExtended.API.Modules
{
    public class ModuleContainer
    {
        public Module Module { get; }
        public ModuleTickStatusInfo TickStatus { get; }

        public ModuleContainer(Module module)
        {
            Module = module;
            TickStatus = new ModuleTickStatusInfo(module.TickSettings.HasValue ? module.TickSettings.Value : new ModuleTickSettings(ModuleTickType.OnUpdate, null, null, null, null));
        }
    }
}