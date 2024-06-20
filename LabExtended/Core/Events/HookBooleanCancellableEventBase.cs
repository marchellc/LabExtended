namespace LabExtended.Core.Events
{
    public class HookBooleanCancellableEventBase : HookCancellableEventBase<bool>
    {
        public override bool AllowedValue => true;
        public override bool DeniedValue => false;

        public HookBooleanCancellableEventBase(bool cancellation = true)
            => Cancellation = cancellation;

        internal override bool IsAllowed(bool value)
            => value;
    }
}