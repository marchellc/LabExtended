namespace LabExtended.Core.Events
{
    public class HookBooleanCancellableEvent : HookCancellableEvent<bool>
    {
        public override bool AllowedValue => true;
        public override bool CancelledValue => false;

        public override bool DefaultValue { get; }

        public HookBooleanCancellableEvent(bool isAllowed)
            => DefaultValue = isAllowed;

        internal override bool IsCancellation(bool value)
            => !value;
    }
}