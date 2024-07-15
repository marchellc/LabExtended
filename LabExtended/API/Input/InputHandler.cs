using Common.Extensions;

using LabExtended.API.Voice;
using LabExtended.API.Input.Interfaces;
using LabExtended.API.Input.Inputs;

using LabExtended.Events.Player;
using LabExtended.Core;
using LabExtended.Extensions;
using LabExtended.Patches.Functions;

using UnityEngine;

using LabExtended.API.Collections.Locked;
using LabExtended.API.Internal;
using LabExtended.API.Enums;

namespace LabExtended.API.Input
{
    public static class InputHandler
    {
        static InputHandler()
        {
            PlayerJoinPatch.OnJoined += OnPlayerJoined;
            PlayerLeavePatch.OnLeaving += OnPlayerLeft;

            VoiceModule.OnStartedSpeaking += OnSpeaking;
            VoiceModule.OnStoppedSpeaking += (player, start, time, capture) => OnSpeaking(player);
        }

        internal static readonly LockedDictionary<Type, InputListenerInfo> _listeners = new LockedDictionary<Type, InputListenerInfo>();
        internal static readonly LockedDictionary<uint, KeybindState> _states = new LockedDictionary<uint, KeybindState>();

        internal static readonly LockedHashSet<KeyCode> _watchedKeys = new LockedHashSet<KeyCode>();

        public static IEnumerable<KeyCode> WatchedKeys => _watchedKeys;
        public static IEnumerable<InputListenerInfo> Listeners => _listeners.Values;

        public static bool RegisterKey(KeyCode key)
        {
            if (_watchedKeys.Contains(key))
                return false;

            _watchedKeys.Add(key);

            foreach (var pair in _states)
            {
                if (pair.Value.SyncedBinds.Contains(key))
                    continue;

                pair.Value.Player.Hub.characterClassManager.TargetChangeCmdBinding(key, $".input {key}");
                pair.Value.SyncedBinds.Add(key);
            }

            return true;
        }

        public static bool UnregisterKey(KeyCode key)
            => _watchedKeys.Remove(key);

        public static T RegisterListener<T>(InputType type) where T : IInputListener
        {
            if (TryGetListener<T>(out var instance))
                return instance;

            if (type is InputType.Keybind)
                throw new InvalidOperationException($"Use the RegisterListener<T>(InputType,KeyCode) method to register a keybind listener.");

            instance = typeof(T).Construct<T>();

            _listeners[typeof(T)] = new InputListenerInfo(instance, type, default);
            return instance;
        }

        public static T RegisterListener<T>(InputType type, KeyCode key) where T : IInputListener
        {
            if (TryGetListener<T>(out var instance))
                return instance;

            if (type != InputType.Keybind)
                throw new InvalidOperationException($"Use the RegisterListener<T>(InputType) method to register a {type} listener.");

            instance = typeof(T).Construct<T>();

            _listeners[typeof(T)] = new InputListenerInfo(instance, type, key);

            if (!_watchedKeys.Contains(key))
                RegisterKey(key);

            return instance;
        }

        public static bool UnregisterListener<T>() where T : IInputListener
            => _listeners.Remove(typeof(T));

        public static void UnregisterListeners()
            => _listeners.Clear();

        public static T GetListener<T>() where T : IInputListener
            => _listeners.TryGetValue(typeof(T), out var listener) ? (T)listener.Listener : throw new Exception($"Unknown listener type: {typeof(T).FullName}");

        public static bool TryGetListener<T>(out T instance) where T : IInputListener
            => ((_listeners.TryGetValue(typeof(T), out var listener) ? instance = (T)listener.Listener : instance = default)) != null;

        internal static void OnWaiting()
            => _states.Clear();

        internal static void OnPlayerJoined(ExPlayer player)
        {
            var state = _states[player.NetId] = new KeybindState() { Player = player };

            foreach (var keyCode in _watchedKeys)
            {
                player.Hub.characterClassManager.TargetChangeCmdBinding(keyCode, $".input {keyCode}");
                state.SyncedBinds.Add(keyCode);
            }
        }

        internal static void OnPlayerLeft(ExPlayer player)
            => _states.Remove(player.NetId);

        internal static void OnPlayerKeybind(ExPlayer player, KeyCode key)
        {
            if (_listeners.Count < 1)
                return;

            var inputInfo = new KeybindInputInfo(player, key);

            foreach (var listener in _listeners)
            {
                if (listener.Value.Type != InputType.Keybind || listener.Value.Key != key)
                    continue;

                try
                {
                    listener.Value.Listener.Trigger(inputInfo);
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Input API", $"Listener &3{listener.Value.Listener.GetType().FullName}&r failed to handle Keybind trigger:\n{ex.ToColoredString()}");
                }
            }
        }

        internal static void OnSpeaking(ExPlayer player)
        {
            if (_listeners.Count < 1)
                return;

            var inputInfo = new VoiceInputInfo(player);

            foreach (var listener in _listeners)
            {
                if (listener.Value.Type != InputType.Voice)
                    continue;

                try
                {
                    listener.Value.Listener.Trigger(inputInfo);
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Input API", $"Listener &3{listener.Value.Listener.GetType().FullName}&r failed to handle Voice trigger:\n{ex.ToColoredString()}");
                }
            }
        }

        internal static void OnTogglingNoClip(PlayerTogglingNoClipArgs args)
        {
            if (_listeners.Count < 1)
                return;

            var inputInfo = new NoClipInputInfo(args.Player);

            foreach (var listener in _listeners)
            {
                if (listener.Value.Type != InputType.NoClip)
                    continue;

                try
                {
                    listener.Value.Listener.Trigger(inputInfo);
                }
                catch (Exception ex)
                {
                    ExLoader.Error("Input API", $"Listener &3{listener.Value.Listener.GetType().FullName}&r failed to handle NoClip trigger:\n{ex.ToColoredString()}");
                }
            }
        }
    }
}