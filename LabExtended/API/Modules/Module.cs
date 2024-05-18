namespace LabExtended.API.Modules
{
    public class Module
    {
        public virtual ModuleTickSettings? TickSettings { get; }

        public ModuleParent Parent { get; internal set; }

        public virtual void Tick() { }

        public virtual void Start() { }
        public virtual void Stop() { }
    }
}