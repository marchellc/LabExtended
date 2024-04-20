namespace LabExtended.Core.Hooking;

public class HookCancellableEvent<T> : HookEvent
{
	internal virtual T CancelledValue { get; }
	internal virtual T AllowedValue { get; }
	
	public HookHandler CancelledBy { get; internal set; }
	public T AllowedStatus { get; internal set; }

	public virtual bool IsCancelled
		=> CancelledBy != null;

	public virtual bool IsAllowed
		=> CancelledBy is null;

	public virtual void CancelWith(T value)
	{
		CancelledBy = CurrentHook;
		AllowedStatus = value;
	}

	public virtual void AllowWith(T value)
	{
		CancelledBy = CurrentHook;
		AllowedStatus = value;
	}

	public virtual void Cancel()
	{
		CancelledBy = CurrentHook;
		AllowedStatus = CancelledValue;
	}

	public virtual void Allow()
	{
		CancelledBy = null;
		AllowedStatus = AllowedValue;
	}

	internal override void Apply(object result)
	{
		base.Apply(result);

		if (result is T tValue)
		{
			if (tValue.Equals(CancelledValue))
			{
				CancelWith(tValue);
				return;
			}

			if (tValue.Equals(AllowedValue))
			{
				AllowWith(tValue);
				return;
			}
		}
	}
}