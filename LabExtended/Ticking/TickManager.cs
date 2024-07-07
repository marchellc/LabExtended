using Common.Caching;
using Common.Extensions;
using Common.IO.Collections;
using Common.Utilities;
using Common.Utilities.Generation;

using LabExtended.Core;
using LabExtended.Core.Profiling;
using LabExtended.Extensions;

using UnityEngine;

namespace LabExtended.Ticking
{
    public static class TickManager
    {
        internal static readonly LockedDictionary<string, TickHandler> _activeTicks = new LockedDictionary<string, TickHandler>();

        internal static TickComponent _component;
        internal static GameObject _object;

        private static readonly ProfilerMarker _globalTick = new ProfilerMarker("Tick Event");
        private static readonly UniqueStringGenerator _idGenerator = new UniqueStringGenerator(new MemoryCache<string>(), 20, false);

        public static event Action OnTick;

        internal static void Init()
        {
            _object ??= new GameObject("Tick Manager Component Object");
            _component ??= _object.AddComponent<TickComponent>();

            UnityEngine.Object.DontDestroyOnLoad(_object);
            UnityEngine.Object.DontDestroyOnLoad(_component);
        }

        internal static void Kill()
        {
            if (_component != null)
            {
                UnityEngine.Object.Destroy(_component);
                _component = null;
            }

            if (_object != null)
            {
                UnityEngine.Object.Destroy(_object);
                _object = null;
            }
        }

        public static bool TryGetHandler(string handlerId, out TickHandler handler)
            => _activeTicks.TryGetValue(handlerId, out handler);

        public static bool TryGetHandler(Action action, out TickHandler handler)
            => (_activeTicks.TryGetFirst(p => p.Value.Method.Method == action.Method && p.Value.Method.Target.IsEqualTo(action.Target), out var activeTick) ? handler = activeTick.Value : handler = null) != null;

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
                ExLoader.Warn("Ticking API", $"Tried to pause an unknown tick: &3{action.Method.ToName()}&r");
                return false;
            }

            if (tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot pause tick &3{action.Method.ToName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;

            ExLoader.Debug("Ticking API", $"Paused tick &3{action.Method.ToName()}&r (&6{tick.Id}&6)");
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
                ExLoader.Warn("Ticking API", $"Cannot pause tick &3{tick.Method.Method.ToName()}&r (&6{tick.Id}&r) - it's already paused.");
                return false;
            }

            tick.IsPaused = true;

            ExLoader.Debug("Ticking API", $"Paused tick &3{tick.Method.Method.ToName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static bool ResumeTick(Action action)
        {
            if (!TryGetHandler(action, out var tick))
            {
                ExLoader.Warn("Tick Manager", $"Tried to pause an unknown tick: &3{action.Method.ToName()}&r");
                return false;
            }

            if (!tick.IsPaused)
            {
                ExLoader.Warn("Ticking API", $"Cannot resume tick &3{action.Method.ToName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;

            ExLoader.Debug("Ticking API", $"Resumed tick &3{action.Method.ToName()}&r (&6{tick.Id}&6)");
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
                ExLoader.Warn("Ticking API", $"Cannot resume tick &3{tick.Method.Method.ToName()}&r (&6{tick.Id}&r) - it's not paused.");
                return false;
            }

            tick.IsPaused = false;

            ExLoader.Debug("Ticking API", $"Resumed tick &3{tick.Method.Method.ToName()}&r (&6{tick.Id}&6)");
            return true;
        }

        public static TickHandler SubscribeTick(Action action, TickOptions options, string customId = null)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            if (options is null)
                throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(customId))
                customId = _idGenerator.Next();

            if (_activeTicks.TryGetFirst(p => p.Value.Method.Method == action.Method && p.Value.Method.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ExLoader.Warn("Ticking API", $"Attempted to register a duplicate tick &3{activeTick.Key}&r (&6{activeTick.Value.Method.Method.ToName()}&r)");
                return null;
            }

            var handler = new TickHandler(customId, action, options);

            handler._marker = new ProfilerMarker($"Tick [{customId}]: {action.Method.DeclaringType?.Name + "." ?? ""}{action.Method.Name}");

            _activeTicks[customId] = handler;

            ExLoader.Debug("Ticking API", $"Subscribed a new tick: &3{action.Method.ToName()}&r (&6{customId}&r)");
            return handler;
        }

        public static bool UnsubscribeTick(Action action)
        {
            if (!_activeTicks.TryGetFirst(p => p.Value.Method.Method == action.Method && p.Value.Method.Target.IsEqualTo(action.Target), out var activeTick))
            {
                ExLoader.Warn("Tick Manager", $"Attempted to unregister an unknown tick (&6{action.Method.ToName()}&r)");
                return false;
            }

            _idGenerator.Free(activeTick.Key);
            return _activeTicks.Remove(activeTick.Key);
        }

        public static bool UnsubscribeTick(string handlerId)
        {
            if (!_activeTicks.TryGetValue(handlerId, out var tickHandler))
                return false;

            _idGenerator.Free(handlerId);
            return _activeTicks.Remove(handlerId);
        }

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
                    if (!pair.Value.CanTick)
                        continue;

                    pair.Value.RegisterTickStart();

                    try
                    {
                        pair.Value.Method();
                    }
                    catch (Exception ex)
                    {
                        ExLoader.Error("Tick Manager", $"Failed to invoke tick &3{pair.Key}&r (&6{pair.Value.Method.Method.ToName()}&r):\n{ex.ToColoredString()}");
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