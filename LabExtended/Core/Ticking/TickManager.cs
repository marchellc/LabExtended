using LabExtended.API.Collections.Locked;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;
using LabExtended.Utilities.Generation;

using System.Collections;

using UnityEngine;

namespace LabExtended.Core.Ticking
{
    /// <summary>
    /// A class that distributes method ticks based on a custom configuration.
    /// </summary>
    public static class TickManager
    {
        internal static readonly LockedHashSet<TickInfo> _activeTicks = new LockedHashSet<TickInfo>();

        internal static TickComponent _component;
        internal static GameObject _object;

        private static readonly ProfilerMarker _globalTick = new ProfilerMarker("Tick Event");
        private static readonly UniqueStringGenerator _idGenerator = new UniqueStringGenerator(20, false);

        private static bool _wasKilled = false;

        /// <summary>
        /// Gets invoked every frame.
        /// </summary>
        public static event Action OnTick;

        internal static void Init()
            => TickComponent.Create("Global Tick", CallUpdate, null, out _object, out _component);

        internal static void Kill()
        {
            _component?.Stop();
            _component = null;

            _wasKilled = true;
        }

        public static bool TryGetHandler(string handlerId, out TickInfo handler)
        {
            return _activeTicks.TryGetFirst(h => h.Id == handlerId, out handler);
        }

        public static bool TryGetHandler(Action action, out TickInfo handler)
        {
            return (_activeTicks.TryGetFirst(p => p.Target.Method == action.Method && p.Target.Target.IsEqualTo(action.Target), out var activeTick) ? handler = activeTick : handler = null) != null;
        }

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
                ApiLoader.Warn("Tick API", $"Tried to pause an unknown tick: &3{action.Method.GetMemberName()}&r");
                return false;
            }

            if (tick.IsPaused)
            {
                ApiLoader.Warn("Tick API", $"Cannot pause tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;
            return true;
        }

        public static bool PauseTick(string handlerId)
        {
            if (!TryGetHandler(handlerId, out var tick))
            {
                ApiLoader.Warn("Tick API", $"Tried to pause an unknown tick: &3{handlerId}&r");
                return false;
            }

            if (tick.IsPaused)
            {
                ApiLoader.Warn("Tick API", $"Cannot pause tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;
            return true;
        }

        public static bool ResumeTick(Action action)
        {
            if (!TryGetHandler(action, out var tick))
            {
                ApiLoader.Warn("Tick API", $"Tried to pause an unknown tick: &3{action.Method.GetMemberName()}&r");
                return false;
            }

            if (!tick.IsPaused)
            {
                ApiLoader.Warn("Tick API", $"Cannot resume tick &3{action.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;
            return true;
        }

        public static bool ResumeTick(string handlerId)
        {
            if (!TryGetHandler(handlerId, out var tick))
            {
                ApiLoader.Warn("Tick API", $"Tried to pause an unknown tick: &3{handlerId}&r");
                return false;
            }

            if (!tick.IsPaused)
            {
                ApiLoader.Warn("Tick API", $"Cannot resume tick &3{tick.Target.Method.GetMemberName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;
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

            if (_activeTicks.TryGetFirst(p => p.Target.Method == action.Method && p.Target.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ApiLoader.Warn("Tick API", $"Attempted to register a duplicate tick &3{activeTick.Id}&r (&6{activeTick.Target.Method.GetMemberName()}&r)");
                return null;
            }

            var handler = new TickInfo(action, customId, options) { IsSeparate = separateCall };

            _activeTicks.Add(handler);

            if (separateCall)
                TickComponent.Create(customId, action, () => handler.CanTick, out _, out handler._separateComponent);
            else if (_wasKilled && _component is null)
            {
                _wasKilled = false;
                TickComponent.Create("Global Tick", CallUpdate, null, out _object, out _component);
            }

            return handler;
        }

        public static bool UnsubscribeTick(Action action)
        {
            if (!_activeTicks.TryGetFirst(p => p.Target.Method == action.Method && p.Target.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ApiLoader.Warn("Tick API", $"Attempted to unregister an unknown tick (&6{action.Method.GetMemberName()}&r)");
                return false;
            }

            if (activeTick.IsSeparate)
                activeTick._separateComponent.Stop();

            _idGenerator.Free(activeTick.Id);
            return _activeTicks.Remove(activeTick);
        }

        public static bool UnsubscribeTick(string handlerId)
        {
            if (!_activeTicks.TryGetFirst(h => h.Id == handlerId, out var tickHandler))
                return false;

            if (tickHandler.IsSeparate)
                tickHandler._separateComponent.Stop();

            _idGenerator.Free(handlerId);
            return _activeTicks.Remove(tickHandler);
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
                        ApiLoader.Error("Tick API", $"Failed to execute the global tick event:\n{ex.ToColoredString()}");
                    }
                }

                foreach (var pair in _activeTicks)
                {
                    if (pair.IsSeparate || !pair.CanTick)
                        continue;

                    pair.RegisterTickStart();

                    try
                    {
                        pair.Target();
                    }
                    catch (Exception ex)
                    {
                        ApiLoader.Error("Tick API", $"Failed to invoke tick &3{pair}&r (&6{pair.Target.Method.GetMemberName()}&r):\n{ex.ToColoredString()}");
                    }

                    pair.RegisterTickEnd();
                }
            }
            catch (Exception ex)
            {
                ApiLoader.Error("Tick API", $"The tick loop caught an exception:\n{ex.ToColoredString()}");
            }
        }
    }
}