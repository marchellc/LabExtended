namespace LabExtended.API.Items.Custom
{
    public abstract class CustomItemBehaviour
    {
        public abstract ExPlayer Owner { get; }
        public abstract bool IsEnabled { get; }

        public CustomItem CustomItem { get; internal set; }

        public ushort ItemSerial { get; internal set; }

        public virtual void OnUpdate() { }
        public virtual void OnEnabled() { }
        public virtual void OnDisabled() { }

        internal virtual void InternalOnUpdate() => OnUpdate();
        internal virtual void InternalOnEnabled() => OnEnabled();
        internal virtual void InternalOnDisabled() => OnDisabled();
    }
}
