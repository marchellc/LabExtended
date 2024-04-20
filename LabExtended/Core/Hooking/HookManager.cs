using Common.Extensions;
using Common.Values;
using Common.IO.Collections;

using LabExtended.Extensions;

using MEC;

using UnityEngine;

namespace LabExtended.Core.Hooking;

public static class HookManager
{
	private static readonly LockedDictionary<object, List<HookHandler>> _hooks = new LockedDictionary<object, List<HookHandler>>();
	private static readonly LockedDictionary<Type, Tuple<RecordedValue<TimeSpan>, RecordedValue<TimeSpan>>> _time = new LockedDictionary<Type, Tuple<RecordedValue<TimeSpan>, RecordedValue<TimeSpan>>>();

	public static bool ExecuteBool<T>(T ev) where T : HookCancellableEvent<bool>
	{
		Execute(ev);
		return ev.AllowedStatus;
	}
	
	public static TCancel Execute<T, TCancel>(T ev) where T : HookCancellableEvent<TCancel>
	{
		Execute(ev);
		return ev.AllowedStatus;
	}

	public static void Execute<T>(T ev) where T : HookEvent
	{
		if (ev is null)
			throw new ArgumentNullException(nameof(ev));

		try
		{
			var args = new object[] { ev };
			var time = DateTime.Now;

			foreach (var pair in _hooks)
			{
				foreach (var hook in pair.Value)
				{
					if (hook.EventType is null || hook.EventType != typeof(T))
						continue;

					try
					{
						ev.OnExecuting(hook);
						ExecuteHook(hook, ev, args, typeof(T));
						ev.OnExecuted(hook);
					}
					catch (Exception ex)
					{
						ExLoader.Error("Hook Manager", $"Failed to execute hook &1'{hook.Method.ToName()}'&r due to an exception:\n{ex.ToColoredString()}");
					}
				}
			}

			if (HookEvent._data.TryGetValue(typeof(T), out var evData) && evData.Item2 != null && evData.Item3 != null)
			{
				var _delegate = evData.Item3.GetValueFast<Delegate>();

				try
				{
					_delegate?.DynamicInvoke(args);
				}
				catch (Exception ex)
				{
					ExLoader.Error("Hook Manager", $"Failed to execute hook delegate for event &2'{typeof(T).Name}'&r due to an exception:\n{ex.ToColoredString()}");
				}
			}

			if (ExLoader.Loader.Config.Hooks.TimeRecording || ExLoader.Loader.Config.Hooks.CustomRecording.Contains(typeof(T).FullName))
			{
				var end = DateTime.Now - time;

				if (_time.TryGetValue(typeof(T), out var record) && record.Item1 != null && record.Item2 != null)
				{
					if (record.Item1.AllValues.Count == 0 || end.TotalMilliseconds > record.Item1.Value.TotalMilliseconds)
						record.Item1.Value = end;

					if (record.Item2.AllValues.Count == 0 || end.TotalMilliseconds < record.Item2.Value.TotalMilliseconds)
						record.Item2.Value = end;
				}
				else
				{
					_time[typeof(T)] = new Tuple<RecordedValue<TimeSpan>, RecordedValue<TimeSpan>>(
						new RecordedValue<TimeSpan>(end),
						new RecordedValue<TimeSpan>(end));
				}

				ExLoader.Debug("Hook Manager", $"Event &2'{typeof(T).Name}'&r finished executing in &3{end.TotalMilliseconds}&r ms");
			}
		}
		catch (Exception ex)
		{
			ExLoader.Error("Hook Manager", $"Event Invocation failed for &2'{typeof(T).Name}'&r due to an exception:\n{ex.ToColoredString()}");
		}
	}

	private static void ExecuteHook(HookHandler hookHandler, HookEvent ev, object[] args, Type type)
	{
		if (hookHandler.Style != HookStyle.EventParameter)
		{
			if (hookHandler.Style is HookStyle.NoParameters)
			{
				args = null;
			}
			else
			{
				args = new object[hookHandler.Binding.Length];

				for (int i = 0; i < hookHandler.Binding.Length; i++)
					args[i] = hookHandler.Binding[i].GetValue(ev);
			}
		}

		if (hookHandler.Executor.MethodReturnType == typeof(void))
		{
			hookHandler.Invoke(args);
		}
		else
		{
			var result = hookHandler.Invoke<object>(args);

			if (result is null)
				return;
			
			if (result is IEnumerator<float> coroutine)
			{
				ExLoader.Debug("Hook Manager", $"Hook Handler {hookHandler.Method.ToName()} returned a coroutine");
				
				var timeout = ExLoader.Loader.Config.Hooks.AsyncTimeout;

				if (ExLoader.Loader.Config.Hooks.CustomTimeouts.TryGetValue(type.FullName, out var evTimeout))
					timeout = evTimeout;

				if (ExLoader.Loader.Config.Hooks.CustomTimeouts.TryGetValue($"{hookHandler.Type.FullName}.{hookHandler.Method.Name}", out evTimeout))
					timeout = evTimeout;

				var isFinished = false;
				var start = DateTime.Now;
				
				ExLoader.Debug("Hook Manager", $"Starting coroutine &2'{hookHandler.Method.ToName()}'&r (timeout: {timeout} ms)");
				
				var handle = ExecuteCoroutine(coroutine, timeout, (time, timedOut) =>
				{
					isFinished = true;
					
					if (hookHandler.TimeRecording)
					{
						var span = TimeSpan.FromMilliseconds(time);

						if (hookHandler.MaxTime.AllValues.Count < 1 || time > hookHandler.MaxTime.Value.TotalMilliseconds)
							hookHandler.MaxTime.Value = span;

						if (hookHandler.MinTime.AllValues.Count < 1 || time < hookHandler.MinTime.Value.TotalMilliseconds)
							hookHandler.MinTime.Value = span;
					}

					if (timedOut)
						ExLoader.Warn("Hook Manager", $"Coroutine &2'{hookHandler.Method.ToName()}'&r has timed out while executing event &2'{type.Name}'&r (timeout: {timeout} ms)!");
					
					ExLoader.Debug("Hook Manager", $"Coroutine &2'{hookHandler.Method.ToName()}'&r has finished executing event &2'{type.Name}'&r in &3{time} ms&r.");
				});
				
				ExLoader.Debug("Hook Manager", $"Waiting for coroutine &2'{hookHandler.Method.ToName()}'&r to finish (timeout: {timeout} ms)");

				while (!isFinished)
				{
					if ((DateTime.Now - start).TotalSeconds >= 5)
					{
						Timing.KillCoroutines(handle);
						ExLoader.Warn("Hook Manager", $"Coroutine &2'{hookHandler.Method.ToName()}'&r has timed out (exceeded 5 seconds) while executing event &2'{type.Name}'&r!");
						break;
					}
					
					continue;
				}
			}
			else
			{
				ev.Apply(result);
			}
		}
	}

	private static CoroutineHandle ExecuteCoroutine(IEnumerator<float> coroutine, float timeout, Action<float, bool> callback)
		=> Timing.RunCoroutine(HookCoroutine(coroutine, timeout, callback));

	private static IEnumerator<float> HookCoroutine(IEnumerator<float> coroutine, float timeout, Action<float, bool> callback)
	{
		var handle = Timing.RunCoroutine(coroutine);
		var time = 0f;

		while (Timing.IsRunning(handle))
		{
			yield return Timing.WaitForOneFrame;
			
			time += Time.time;

			if (timeout > 0f && time >= timeout)
			{
				Timing.KillCoroutines(handle);
				callback(time, true);
				yield break;
			}
		}

		callback(time, false);
	}
}