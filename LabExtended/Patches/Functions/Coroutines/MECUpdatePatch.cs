using System.Collections;
using HarmonyLib;

using LabExtended.Core;

using MEC;

using UnityEngine;

namespace LabExtended.Patches.Functions.Coroutines;

public static class MECUpdatePatch
{
	[HarmonyPatch(typeof(Timing), nameof(Timing.Update))]
	public static bool UpdatePrefix(Timing __instance)
	{
		try
		{
			if (__instance._lastSlowUpdateTime + __instance.TimeBetweenSlowUpdateCalls < Time.realtimeSinceStartup &&
			    __instance._nextSlowUpdateProcessSlot > 0)
			{
				var processIndex = new Timing.ProcessIndex { seg = Segment.SlowUpdate };

				if (__instance.UpdateTimeValues(processIndex.seg))
					__instance._lastSlowUpdateProcessSlot = __instance._nextSlowUpdateProcessSlot;

				processIndex.i = 0;

				while (processIndex.i < __instance._lastSlowUpdateProcessSlot)
				{
					try
					{
						if (!__instance.SlowUpdatePaused[processIndex.i] &&
						    !__instance.SlowUpdateHeld[processIndex.i] &&
						    __instance.SlowUpdateProcesses[processIndex.i] != null &&
						    __instance.localTime >= __instance.SlowUpdateProcesses[processIndex.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex];

							if (!__instance.SlowUpdateProcesses[processIndex.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex]);
								}
							}
							else if (__instance.SlowUpdateProcesses[processIndex.i] != null &&
							         float.IsNaN(__instance.SlowUpdateProcesses[processIndex.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.SlowUpdateProcesses[processIndex.i] =
										Timing.ReplacementFunction(__instance.SlowUpdateProcesses[processIndex.i],
											__instance._indexToHandle[processIndex]);
									Timing.ReplacementFunction = null;
								}

								processIndex.i--;
							}
						}
					}
					catch (Exception ex)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[SLOW UPDATE]&r &1\n{ex}&r");
					}

					processIndex.i++;
				}
			}

			if (__instance._nextRealtimeUpdateProcessSlot > 0)
			{
				var processIndex2 = new Timing.ProcessIndex { seg = Segment.RealtimeUpdate };

				if (__instance.UpdateTimeValues(processIndex2.seg))
					__instance._lastRealtimeUpdateProcessSlot = __instance._nextRealtimeUpdateProcessSlot;

				processIndex2.i = 0;

				while (processIndex2.i < __instance._lastRealtimeUpdateProcessSlot)
				{
					try
					{
						if (!__instance.RealtimeUpdatePaused[processIndex2.i] &&
						    !__instance.RealtimeUpdateHeld[processIndex2.i] &&
						    __instance.RealtimeUpdateProcesses[processIndex2.i] != null && __instance.localTime >=
						    __instance.RealtimeUpdateProcesses[processIndex2.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex2];

							if (!__instance.RealtimeUpdateProcesses[processIndex2.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex2))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex2]);
								}
							}
							else if (__instance.RealtimeUpdateProcesses[processIndex2.i] != null &&
							         float.IsNaN(__instance.RealtimeUpdateProcesses[processIndex2.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.RealtimeUpdateProcesses[processIndex2.i] =
										Timing.ReplacementFunction(__instance.RealtimeUpdateProcesses[processIndex2.i],
											__instance._indexToHandle[processIndex2]);
									Timing.ReplacementFunction = null;
								}

								processIndex2.i--;
							}
						}
					}
					catch (Exception ex2)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[REAL TIME UPDATE]&r &1\n{ex2}&r");
					}

					processIndex2.i++;
				}
			}

			if (__instance._nextUpdateProcessSlot > 0)
			{
				var processIndex3 = new Timing.ProcessIndex { seg = Segment.Update };

				if (__instance.UpdateTimeValues(processIndex3.seg))
					__instance._lastUpdateProcessSlot = __instance._nextUpdateProcessSlot;

				processIndex3.i = 0;

				while (processIndex3.i < __instance._lastUpdateProcessSlot)
				{
					try
					{
						if (!__instance.UpdatePaused[processIndex3.i] && !__instance.UpdateHeld[processIndex3.i] &&
						    __instance.UpdateProcesses[processIndex3.i] != null &&
						    __instance.localTime >= __instance.UpdateProcesses[processIndex3.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex3];

							if (!__instance.UpdateProcesses[processIndex3.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex3))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex3]);
								}
							}
							else if (__instance.UpdateProcesses[processIndex3.i] != null &&
							         float.IsNaN(__instance.UpdateProcesses[processIndex3.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.UpdateProcesses[processIndex3.i] =
										Timing.ReplacementFunction(__instance.UpdateProcesses[processIndex3.i],
											__instance._indexToHandle[processIndex3]);
									Timing.ReplacementFunction = null;
								}

								processIndex3.i--;
							}
						}
					}
					catch (Exception ex3)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[UPDATE]&r &1\n{ex3}&r");
					}

					processIndex3.i++;
				}
			}

			if (__instance.AutoTriggerManualTimeframe)
			{
				__instance.TriggerManualTimeframeUpdate();
			}
			else
			{
				var num = (ushort)(__instance._framesSinceUpdate + 1);

				__instance._framesSinceUpdate = num;

				if (num > 64)
				{
					__instance._framesSinceUpdate = 0;
					__instance.RemoveUnused();
				}
			}

			__instance.currentCoroutine = default;
		}
		catch (Exception ex)
		{
			ApiLog.Error("More Effective Coroutines", $"&3[UPDATE]&r &1\n{ex}&r");
		}

		return false;
	}

	[HarmonyPatch(typeof(Timing), nameof(Timing.FixedUpdate))]
	public static bool FixedUpdatePrefix(Timing __instance)
	{
		try
		{
			if (__instance._nextFixedUpdateProcessSlot > 0)
			{
				var processIndex = new Timing.ProcessIndex { seg = Segment.FixedUpdate };

				if (__instance.UpdateTimeValues(processIndex.seg))
					__instance._lastFixedUpdateProcessSlot = __instance._nextFixedUpdateProcessSlot;

				processIndex.i = 0;

				while (processIndex.i < __instance._lastFixedUpdateProcessSlot)
				{
					try
					{
						if (!__instance.FixedUpdatePaused[processIndex.i] &&
						    !__instance.FixedUpdateHeld[processIndex.i] &&
						    __instance.FixedUpdateProcesses[processIndex.i] != null &&
						    __instance.localTime >= __instance.FixedUpdateProcesses[processIndex.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex];

							if (!__instance.FixedUpdateProcesses[processIndex.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex]);
								}
							}
							else if (__instance.FixedUpdateProcesses[processIndex.i] != null &&
							         float.IsNaN(__instance.FixedUpdateProcesses[processIndex.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.FixedUpdateProcesses[processIndex.i] =
										Timing.ReplacementFunction(__instance.FixedUpdateProcesses[processIndex.i],
											__instance._indexToHandle[processIndex]);
									Timing.ReplacementFunction = null;
								}

								processIndex.i--;
							}
						}
					}
					catch (Exception ex)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[FIXED UPDATE]&r &1\n{ex}&r");
					}

					processIndex.i++;
				}

				__instance.currentCoroutine = default;
			}
		}
		catch (Exception ex)
		{
			ApiLog.Error("More Effective Coroutines", $"&3[FIXED UPDATE]&r &1\n{ex}&r");
		}

		return false;
	}

	[HarmonyPatch(typeof(Timing), nameof(Timing.LateUpdate))]
	public static bool LateUpdatePrefix(Timing __instance)
	{
		try
		{
			if (__instance._nextLateUpdateProcessSlot > 0)
			{
				var processIndex = new Timing.ProcessIndex { seg = Segment.LateUpdate };
				
				if (__instance.UpdateTimeValues(processIndex.seg))
					__instance._lastLateUpdateProcessSlot = __instance._nextLateUpdateProcessSlot;

				processIndex.i = 0;
				
				while (processIndex.i < __instance._lastLateUpdateProcessSlot)
				{
					try
					{
						if (!__instance.LateUpdatePaused[processIndex.i] && !__instance.LateUpdateHeld[processIndex.i] &&
						    __instance.LateUpdateProcesses[processIndex.i] != null &&
						    __instance.localTime >= __instance.LateUpdateProcesses[processIndex.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex];

							if (!__instance.LateUpdateProcesses[processIndex.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex]);
								}
							}
							else if (__instance.LateUpdateProcesses[processIndex.i] != null &&
							         float.IsNaN(__instance.LateUpdateProcesses[processIndex.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.LateUpdateProcesses[processIndex.i] =
										Timing.ReplacementFunction(__instance.LateUpdateProcesses[processIndex.i],
											__instance._indexToHandle[processIndex]);
									Timing.ReplacementFunction = null;
								}

								processIndex.i--;
							}
						}
					}
					catch (Exception ex)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[LATE UPDATE]&r &1\n{ex}&r");
					}

					processIndex.i++;
				}

				__instance.currentCoroutine = default;
			}
		}
		catch (Exception ex)
		{
			ApiLog.Error("More Effective Coroutines", $"&3[LATE UPDATE]&r &1\n{ex}&r");
		}

		return false;
	}

	[HarmonyPatch(typeof(Timing), nameof(Timing.TriggerManualTimeframeUpdate))]
	public static bool ManualPrefix(Timing __instance)
	{
		try
		{
			if (__instance._nextManualTimeframeProcessSlot > 0)
			{
				var processIndex = new Timing.ProcessIndex { seg = Segment.ManualTimeframe };
				
				if (__instance.UpdateTimeValues(processIndex.seg))
					__instance._lastManualTimeframeProcessSlot = __instance._nextManualTimeframeProcessSlot;

				processIndex.i = 0;
				
				while (processIndex.i < __instance._lastManualTimeframeProcessSlot)
				{
					try
					{
						if (!__instance.ManualTimeframePaused[processIndex.i] && !__instance.ManualTimeframeHeld[processIndex.i] &&
						    __instance.ManualTimeframeProcesses[processIndex.i] != null && __instance.localTime >=
						    __instance.ManualTimeframeProcesses[processIndex.i].Current)
						{
							__instance.currentCoroutine = __instance._indexToHandle[processIndex];

							if (!__instance.ManualTimeframeProcesses[processIndex.i].MoveNext())
							{
								if (__instance._indexToHandle.ContainsKey(processIndex))
								{
									__instance.KillCoroutinesOnInstance(__instance._indexToHandle[processIndex]);
								}
							}
							else if (__instance.ManualTimeframeProcesses[processIndex.i] != null &&
							         float.IsNaN(__instance.ManualTimeframeProcesses[processIndex.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									__instance.ManualTimeframeProcesses[processIndex.i] =
										Timing.ReplacementFunction(__instance.ManualTimeframeProcesses[processIndex.i],
											__instance._indexToHandle[processIndex]);
									Timing.ReplacementFunction = null;
								}

								processIndex.i--;
							}
						}
					}
					catch (Exception ex)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[MANUAL UPDATE]&r &1\n{ex}&r");
					}

					processIndex.i++;
				}
			}

			var num = (ushort)(__instance._framesSinceUpdate + 1);
			
			__instance._framesSinceUpdate = num;
			
			if (num > 64)
			{
				__instance._framesSinceUpdate = 0;
				__instance.RemoveUnused();
			}

			__instance.currentCoroutine = default;
		}
		catch (Exception ex)
		{
			ApiLog.Error("More Effective Coroutines", $"&3[MANUAL UPDATE]&r &1\n{ex}&r");
		}

		return false;
	}

	private static IEnumerator<float> EndOfFramePumpWatcher(Timing timing, MonoBehaviour behaviour)
	{
		while (timing._nextEndOfFrameProcessSlot > 0)
		{
			if (!timing._EOFPumpRan)
				behaviour.StartCoroutine(EndOfFramePump(timing));
			
			timing._EOFPumpRan = false;
			yield return float.NegativeInfinity;
		}
		
		timing._EOFPumpRan = false;
	}

	private static IEnumerator EndOfFramePump(Timing timing)
	{
		while (timing._nextEndOfFrameProcessSlot > 0)
		{
			yield return Timing.EofWaitObject;

			try
			{
				var processIndex = new Timing.ProcessIndex { seg = Segment.EndOfFrame };

				timing._EOFPumpRan = true;

				if (timing.UpdateTimeValues(processIndex.seg))
					timing._lastEndOfFrameProcessSlot = timing._nextEndOfFrameProcessSlot;

				processIndex.i = 0;

				while (processIndex.i < timing._lastEndOfFrameProcessSlot)
				{
					try
					{
						if (!timing.EndOfFramePaused[processIndex.i] && !timing.EndOfFrameHeld[processIndex.i] &&
						    timing.EndOfFrameProcesses[processIndex.i] != null &&
						    timing.localTime >= timing.EndOfFrameProcesses[processIndex.i].Current)
						{
							timing.currentCoroutine = timing._indexToHandle[processIndex];

							if (!timing.EndOfFrameProcesses[processIndex.i].MoveNext())
							{
								if (timing._indexToHandle.ContainsKey(processIndex))
								{
									timing.KillCoroutinesOnInstance(timing._indexToHandle[processIndex]);
								}
							}
							else if (timing.EndOfFrameProcesses[processIndex.i] != null &&
							         float.IsNaN(timing.EndOfFrameProcesses[processIndex.i].Current))
							{
								if (Timing.ReplacementFunction != null)
								{
									timing.EndOfFrameProcesses[processIndex.i] = Timing.ReplacementFunction(
										timing.EndOfFrameProcesses[processIndex.i],
										timing._indexToHandle[processIndex]);
									Timing.ReplacementFunction = null;
								}

								processIndex.i--;
							}
						}
					}
					catch (Exception ex)
					{
						ApiLog.Error("More Effective Coroutines", $"&3[END OF FRAME]&r &1\n{ex}&r");
					}

					processIndex.i++;
				}
			}
			catch (Exception ex)
			{
				ApiLog.Error("More Effective Coroutines", $"&3[END OF FRAME]&r &1\n{ex}&r");
			}
		}

		timing.currentCoroutine = default;
	}

	[HarmonyPatch(typeof(Timing), nameof(Timing.OnEnable))]
	private static void EnablePrefix(Timing __instance)
	{
		Timing.MainThread ??= Thread.CurrentThread;
		
		if (__instance._nextEditorUpdateProcessSlot > 0 || __instance._nextEditorSlowUpdateProcessSlot > 0)
			__instance.OnEditorStart();
		
		__instance.InitializeInstanceID();
		
		if (__instance._nextEndOfFrameProcessSlot > 0)
			__instance.RunCoroutineSingletonOnInstance(EndOfFramePumpWatcher(__instance, __instance), "MEC_EOFPumpWatcher", SingletonBehavior.Abort);
	}

	[HarmonyPatch(typeof(Timing), nameof(Timing.RunCoroutineInternal))]
	private static bool RunCoroutineInternalPrefix(Timing __instance, 
		IEnumerator<float> coroutine, Segment segment, int layer, 
		bool layerHasValue, string tag, CoroutineHandle handle, bool prewarm,
		ref CoroutineHandle __result)
	{
		if (segment is Segment.EndOfFrame)
		{
			try
			{
				var processIndex = new Timing.ProcessIndex { seg = segment };

				if (__instance._handleToIndex.ContainsKey(handle))
				{
					__instance._indexToHandle.Remove(__instance._handleToIndex[handle]);
					__instance._handleToIndex.Remove(handle);
				}

				var num = __instance.localTime;
				var num2 = __instance.deltaTime;

				var currentCoroutine = __instance.currentCoroutine;

				__instance.currentCoroutine = handle;

				if (__instance._nextEndOfFrameProcessSlot >= __instance.EndOfFrameProcesses.Length)
				{
					var endOfFrameProcesses = __instance.EndOfFrameProcesses;
					var endOfFramePaused = __instance.EndOfFramePaused;
					var endOfFrameHeld = __instance.EndOfFrameHeld;

					var num15 = (ushort)__instance.EndOfFrameProcesses.Length;
					var num16 = 64;
					var num5 = __instance._expansions;

					__instance._expansions++;

					__instance.EndOfFrameProcesses = new IEnumerator<float>[(num15 + num16 * num5)];
					__instance.EndOfFramePaused = new bool[__instance.EndOfFrameProcesses.Length];
					__instance.EndOfFrameHeld = new bool[__instance.EndOfFrameProcesses.Length];

					for (int n = 0; n < endOfFrameProcesses.Length; n++)
					{
						__instance.EndOfFrameProcesses[n] = endOfFrameProcesses[n];
						__instance.EndOfFramePaused[n] = endOfFramePaused[n];
						__instance.EndOfFrameHeld[n] = endOfFrameHeld[n];
					}
				}

				if (__instance.UpdateTimeValues(processIndex.seg))
					__instance._lastEndOfFrameProcessSlot = __instance._nextEndOfFrameProcessSlot;

				var num6 = __instance._nextEndOfFrameProcessSlot;

				__instance._nextEndOfFrameProcessSlot = num6 + 1;

				processIndex.i = num6;

				__instance.EndOfFrameProcesses[processIndex.i] = coroutine;

				if (tag != null)
					__instance.AddTagOnInstance(tag, handle);

				if (layerHasValue)
					__instance.AddLayerOnInstance(layer, handle);

				__instance._indexToHandle.Add(processIndex, handle);
				__instance._handleToIndex.Add(handle, processIndex);
				__instance._eofWatcherHandle = __instance.RunCoroutineSingletonOnInstance(
					EndOfFramePumpWatcher(__instance, __instance), __instance._eofWatcherHandle,
					SingletonBehavior.Abort);

				__instance.localTime = num;
				__instance.deltaTime = num2;

				__instance.currentCoroutine = currentCoroutine;
				__result = handle;
			}
			catch (Exception ex)
			{
				ApiLog.Error("More Effective Coroutines", $"&3[RUN COROUTINE INTERNAL]&r &1\n{ex}&r");
			}

			return false;
		}

		return true;
	}
}