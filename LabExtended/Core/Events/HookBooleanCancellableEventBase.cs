namespace LabExtended.Core.Events
{
    /// <summary>
    /// Represents an event that can be cancelled by a <see langword="bool"/>.
    /// </summary>
    public class HookBooleanCancellableEventBase : HookCancellableEventBase<bool>
    {
        /// <inheritdoc/>
        public override bool AllowedValue => true;

        /// <inheritdoc/>
        public override bool DeniedValue => false;

        internal HookBooleanCancellableEventBase(bool cancellation = true)
            => Cancellation = cancellation;

        internal override bool IsAllowed(bool value)
            => value;
    }
}