using System.Reflection;

using Common.Utilities;
using Common.Values;

namespace LabExtended.Core.Hooking;

public class HookHandler
{
	public MethodInfo Method
	{
		get => Executor.MethodInfo;
	}

	public Type Type
	{
		get => Executor.MethodInfo.DeclaringType;
	}
	
	public ObjectMethodExecutor Executor { get; }
	public object Target { get; }
	public PropertyInfo[] Binding { get; }
	public Type EventType { get; set; }
	public HookStyle Style { get; }
	
	public bool TimeRecording { get; set; }

	public RecordedValue<TimeSpan> MaxTime { get; } = new RecordedValue<TimeSpan>();
	public RecordedValue<TimeSpan> MinTime { get; } = new RecordedValue<TimeSpan>();

	public TimeSpan AverageTime
	{
		get => TimeSpan.FromMilliseconds((MaxTime.AllValues.Average(t => t.TotalMilliseconds) + MinTime.AllValues.Average(t => t.TotalMilliseconds)) / 2);
	}

	public HookHandler(ObjectMethodExecutor objectMethodExecutor, object target, HookStyle hookStyle, PropertyInfo[] binding)
	{
		Executor = objectMethodExecutor;
		Target = target;
		Binding = binding;
		Style = hookStyle;
	}

	public void Invoke(object[] args)
		=> InvokeInternal(args);

	public T Invoke<T>(object[] args)
		=> (T)InvokeInternal(args);

	private object InvokeInternal(object[] args)
	{
		var start = DateTime.Now;
		var result = Executor.Execute(Target, args);

		if (TimeRecording)
		{
			var time = DateTime.Now - start;

			if (MaxTime.AllValues.Count < 1 || time > MaxTime.Value)
				MaxTime.Value = time;

			if (MinTime.AllValues.Count < 1 || time < MinTime.Value)
				MinTime.Value = time;
		}

		return result;
	}
}