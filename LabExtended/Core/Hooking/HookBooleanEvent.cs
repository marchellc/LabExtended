namespace LabExtended.Core.Hooking;

public class HookBooleanEvent : HookCancellableEvent<bool>
{
	internal override bool AllowedValue => true;
	internal override bool CancelledValue => false;
}