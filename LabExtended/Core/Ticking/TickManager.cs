using LabExtended.API.Collections.Locked;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;
using LabExtended.Utilities.Generation;
using System.Collections;
using UnityEngine;

namespace LabExtended.Core.Ticking
{
    public static class TickManager
    {
        internal static readonly LockedDictionary<string, TickInfo> _activeTicks = new LockedDictionary<string, TickInfo>();

        internal static TickComponent _component;
        internal static GameObject _object;

        private static readonly ProfilerMarker _globalTick = new ProfilerMarker("Tick Event");
        private static readonly UniqueStringGenerator _idGenerator = new UniqueStringGenerator(20, false);

        public static event Action OnTick;

        internal static void Init()
            => TickComponent.Create("Global Tick", CallUpdate, null, out _object, out _component);

        internal static void Kill()
            => _component?.Stop();

        public static bool TryGetHandler(string handlerId, out TickInfo handler)
            => _activeTicks.TryGetValue(handlerId, out handler);

        public static bool TryGetHandler(Action action, out TickInfo handler)
            => (_activeTicks.TryGetFirst(p => p.Value.Target.Method == action.Method && p.Value.Target.Target.IsEqualTo(action.Target), out var activeTick) ? handler = activeTick.Value : handler = null) != null;

        public static bool IsPaused(Action action)
            => TryGetHandler(action, out var tick) && tick.IsPaused;

        public static bool IsPaused(string handlerId)
            => TryGetHandler(handlerId, out var tick) && tick.IsPaused;

        public static bool IsRunning(Action action)
            => TryGetHandler(action, out var tick) && !tick.IsPaused;

        public static bool IsRunning(string handlerId)
            => TryGetHandler(handlerId, out var tick) && !tick.IsPaused;

        public static bool PauseTick(Action action)
        {
            if (!TryGetHandler(action, out var tick))
            {
                ExLoader.Warn("Ticking API", $"Tried to pause an unknown tick: &3{action.Method.GetMemberName()}&r");
                return false;
            }

            if (tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot pause tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;

            ExLoader.Debug("Ticking API", $"Paused tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static bool PauseTick(string handlerId)
        {
            if (!TryGetHandler(handlerId, out var tick))
            {
                ExLoader.Warn("Tick Manager", $"Tried to pause an unknown tick: &3{handlerId}&r");
                return false;
            }

            if (tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot pause tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;

            ExLoader.Debug("Ticking API", $"Paused tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static bool ResumeTick(Action action)
        {
            if (!TryGetHandler(action, out var tick))
            {
                ExLoader.Warn("Tick Manager", $"Tried to pause an unknown tick: &3{action.Method.GetMemberName()}&r");
                return false;
            }

            if (!tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot resume tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;

            ExLoader.Debug("Ticking API", $"Resumed tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static bool ResumeTick(string handlerId)
        {
            if (!TryGetHandler(handlerId, out var tick))
            {
                ExLoader.Warn("Tick Manager", $"Tried to pause an unknown tick: &3{handlerId}&r");
                return false;
            }

            if (!tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot resume tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;

            ExLoader.Debug("Ticking API", $"Resumed tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static TickInfo SubscribeTick(Action action, TickTimer options, string customId = null, bool separateCall = false)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(customId))
                customId = _idGenerator.Next();

            if (_activeTicks.TryGetFirst(p => p.Value.Target.Method == action.Method && p.Value.Target.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ExLoader.Warn("Ticking API", $"Attempted to register a duplicate tick &3{activeTick.Key}&r (&6{activeTick.Value.Target.Method.GetMemberName()}&r)");
                return null;
            }

            var handler = new TickInfo(action, customId, options);

            handler.IsSeparate = separateCall;

            _activeTicks[customId] = handler;

            if (separateCall)
                TickComponent.Create(customId, action, () => handler.CanTick, out _, out handler._separateComponent);

            ExLoader.Debug("Ticking API", $"Subscribed a new tick: &3{action.Method.GetMemberName()}&r (&6{customId}&r)");
            return handler;
        }

        public static bool UnsubscribeTick(Action action)
        {
            if (!_activeTicks.TryGetFirst(p => p.Value.Target.Method == action.Method && p.Value.Target.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ExLoader.Warn("Tick Manager", $"Attempted to unregister an unknown tick (&6{action.Method.GetMemberName()}&r)");
                return false;
            }

            if (activeTick.Value.IsSeparate)
                activeTick.Value._separateComponent.Stop();

            _idGenerator.Free(activeTick.Key);
            return _activeTicks.Remove(activeTick.Key);
        }

        public static bool UnsubscribeTick(string handlerId)
        {
            if (!_activeTicks.TryGetValue(handlerId, out var tickHandler))
                return false;

            if (tickHandler.IsSeparate)
                tickHandler._separateComponent.Stop();

            _idGenerator.Free(handlerId);
            return _activeTicks.Remove(handlerId);
        }

        public static Coroutine RunCoroutine(IEnumerator coroutine)
            => _component.StartCoroutine(coroutine);

        public static void StopCoroutine(Coroutine coroutine)
            => _component.StopCoroutine(coroutine);

        internal static void CallUpdate()
        {
            try
            {
                if (OnTick != null)
                {
                    try
                    {
                        _globalTick.MarkStart();

                        OnTick();

                        _globalTick.MarkEnd();
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Tick Manager", $"Failed to execute the global tick event:\n{ex.ToColoredString()}");
                    }
                }

                foreach (var pair in _activeTicks)
                {
                    if (pair.Value.IsSeparate || !pair.Value.CanTick)
                        continue;

                    pair.Value.RegisterTickStart();

                    try
                    {
                        pair.Value.Target();
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Tick Manager", $"Failed to invoke tick &3{pair.Key}&r (&6{pair.Value.Target.Method.GetMemberName()}&r):\n{ex.ToColoredString()}");
                    }

                    pair.Value.RegisterTickEnd();
                }
            }
            catch (Exception ex)
            {
                ExLoader.Error("Tick Manager", $"The tick loop caught an exception:\n{ex.ToColoredString()}");
            }
        }
    }
}