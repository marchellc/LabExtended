using System.Reflection;

using Common.Extensions;
using Common.IO.Collections;

using LabExtended.Hooks;

namespace LabExtended.Core.Hooking;

public class HookEvent
{
	internal static readonly LockedDictionary<Type, Tuple<PropertyInfo[], EventInfo, FieldInfo>> _data =
		new LockedDictionary<Type, Tuple<PropertyInfo[], EventInfo, FieldInfo>>();

	public HookEvent()
	{
		var type = GetType();

		if (!_data.ContainsKey(type))
		{
			var props = type.GetAllProperties();
			var ev = typeof(HookDelegates).GetAllEvents().FirstOrDefault(ev => ev.EventHandlerType.GetFirstGenericType() == type);
			var field = default(FieldInfo);
			
			if (ev != null)
				field = typeof(HookDelegates).GetAllFields().FirstOrDefault(f => f.Name == ev.Name);

			_data[type] = new Tuple<PropertyInfo[], EventInfo, FieldInfo>(props, ev, field);
		}
	}
	
	public HookHandler CurrentHook { get; internal set; }
	public HookHandler PreviousHook { get; internal set; }

	internal virtual void OnExecuting(HookHandler hook)
		=> CurrentHook = hook;

	internal virtual void OnExecuted(HookHandler hook)
	{
		PreviousHook = hook;
		CurrentHook = null;
	}
	
	internal virtual void Apply(object result) { }
}