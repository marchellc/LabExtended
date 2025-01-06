using CustomPlayerEffects;

using CustomRendering;

using InventorySystem.Items.Usables.Scp244.Hypothermia;

using LabExtended.Core;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using System.Reflection;
using LabExtended.Core.Pooling.Pools;
using UnityEngine;

namespace LabExtended.API.Containers
{
    public class EffectContainer
    {
        private static readonly IEnumerable<PropertyInfo> _properties;

        static EffectContainer()
            => _properties = typeof(EffectContainer).FindProperties(x => (x.GetSetMethod(true)?.IsPrivate ?? false));

        public Dictionary<Type, StatusEffectBase> Effects { get; }

        public PlayerEffectsController Controller { get; }

        public int Count => Effects.Count;

        public int ActiveEffectsCount => Effects.Count(x => x.Value.IsEnabled);
        public int InactiveEffectsCount => Effects.Count(x => !x.Value.IsEnabled);

        public IEnumerable<StatusEffectBase> ActiveEffects => GetEffects(x => x.IsEnabled);
        public IEnumerable<StatusEffectBase> InactiveEffects => GetEffects(x => !x.IsEnabled);

        public AmnesiaItems AmnesiaItems { get; private set; }
        public AmnesiaVision AmnesiaVision { get; private set; }

        public AntiScp207 AntiScp207 { get; private set; }
        public Asphyxiated Asphyxiated { get; private set; }

        public Burned Burned { get; private set; }
        public Blurred Blurred { get; private set; }
        public Bleeding Bleeding { get; private set; }
        public Blindness Blindness { get; private set; }
        public BodyshotReduction BodyshotReduction { get; private set; }

        public CardiacArrest CardiacArrest { get; private set; }
        public Concussed Concussed { get; private set; }
        public Corroding Corroding { get; private set; }

        public DamageReduction DamageReduction { get; private set; }
        public Decontaminating Decontaminating { get; private set; }
        public Deafened Deafened { get; private set; }
        public Disabled Disabled { get; private set; }

        public Ensnared Ensnared { get; private set; }
        public Exhausted Exhausted { get; private set; }

        public Flashed Flashed { get; private set; }
        public FogControl FogControl { get; private set; }

        public Ghostly Ghostly { get; private set; }

        public Hemorrhage Hemorrhage { get; private set; }
        public Hypothermia Hypothermia { get; private set; }

        public InsufficientLighting InsufficientLighting { get; private set; }
        public Invigorated Invigorated { get; private set; }
        public Invisible Invisible { get; private set; }

        public MovementBoost MovementBoost { get; private set; }

        public PocketCorroding PocketCorroding { get; private set; }
        public Poisoned Poisoned { get; private set; }
        public PitDeath PitDeath { get; private set; }

        public RainbowTaste RainbowTaste { get; private set; }

        public Scp207 Scp207 { get; private set; }
        public Scp1853 Scp1853 { get; private set; }
        public Scp1344 Scp1344 { get; private set; }
        public Scanned Scanned { get; private set; }
        public Stained Stained { get; private set; }
        public Sinkhole Sinkhole { get; private set; }
        public Slowness Slowness { get; private set; }
        public Strangled Strangled { get; private set; }
        public SilentWalk SilentWalk { get; private set; }
        public SeveredEyes SeveredEyes { get; private set; }
        public SeveredHands SeveredHands { get; private set; }
        public SoundtrackMute SoundtrackMute { get; private set; }
        public SpawnProtected SpawnProtected { get; private set; }

        public Traumatized Traumatized { get; private set; }

        public Vitality Vitality { get; private set; }

        public bool HasForcedFog => FogControl.Intensity > 0;

        public bool HasMutedSoundtrack
        {
            get => SoundtrackMute.IsEnabled;
            set => SoundtrackMute.IsEnabled = value;
        }

        public FogType ForcedFog
        {
            get => HasForcedFog ? (FogType)(FogControl.Intensity - 1) : FogType.None;
            set => FogControl.SetFogType(value);
        }

        public EffectContainer(PlayerEffectsController controller)
        {
            try
            {
                var dict = new Dictionary<Type, StatusEffectBase>();
                var props = ListPool<PropertyInfo>.Shared.Rent();

                foreach (var effect in controller.AllEffects)
                {
                    if (effect is null)
                        continue;

                    var type = effect.GetType();

                    if (dict.ContainsKey(type))
                        continue;

                    dict.Add(type, effect);

                    if (_properties.TryGetFirst(x => x.PropertyType == type, out var property))
                    {
                        property.SetValue(this, effect);
                        props.Add(property);
                    }
                    else
                    {
                        ApiLog.Warn("Effect API", $"No properties are defined for effect: {type.FullName}");
                    }
                }

                Effects = dict;
                Controller = controller;

                if (props.Count != _properties.Count())
                {
                    ApiLog.Warn("Effect API", $"Failed to set some effect properties (total={_properties.Count()} / set={props.Count})");

                    foreach (var prop in _properties)
                    {
                        if (props.Contains(prop))
                            continue;

                        ApiLog.Warn("Effect API", $"Missing effect for property: {prop.GetMemberName()}");
                    }
                }

                ListPool<PropertyInfo>.Shared.Return(props);
            }
            catch (Exception ex)
            {
                ApiLog.Error("Effect API", $"An error occurred while setting up the effect container!\n{ex.ToColoredString()}");
            }
        }

        public void EnableAllEffects()
            => Effects.ForEach(x => x.Value.ServerSetState(1));

        public void DisableAllEffects()
            => Effects.ForEach(x => x.Value.ServerDisable());

        public void DisableActiveEffects()
            => ActiveEffects.ForEach(x => x.ServerDisable());

        public void EnableInactiveEffects()
            => InactiveEffects.ForEach(x => x.ServerSetState(1));

        #region IsActive
        public bool IsActive<T>() where T : StatusEffectBase
            => TryGetEffect<T>(out var effect) && effect.IsEnabled;

        public bool IsActive(Type type)
            => TryGetEffect(type, out var effect) && effect.IsEnabled;

        public bool IsActive(string name, bool lowerCase = true)
            => TryGetEffect(name, lowerCase, out var effect) && effect.IsEnabled;
        #endregion

        #region EnableEffect
        public void EnableEffect<T>(byte intensity, float duration = 0f, bool addDurationIfActive = false) where T : StatusEffectBase
            => GetEffect<T>()?.ServerSetState(intensity, duration, addDurationIfActive);

        public void EnableEffect(Type type, byte intensity, float duration = 0f, bool addDurationIfActive = false)
            => GetEffect(type)?.ServerSetState(intensity, duration, addDurationIfActive);

        public void EnableEffect(string name, byte intensity, float duration = 0f, bool addDurationIfActive = false)
            => GetEffect(name)?.ServerSetState(intensity, duration, addDurationIfActive);
        #endregion

        #region DisableEffect
        public void DisableEffect<T>() where T : StatusEffectBase
            => GetEffect<T>()?.ServerDisable();

        public void DisableEffect(Type type)
            => GetEffect(type)?.ServerDisable();

        public void DisableEffect(string name, bool lowerCase = true)
            => GetEffect(name, lowerCase)?.ServerDisable();
        #endregion

        #region GetIntensity
        public byte GetIntensity<T>() where T : StatusEffectBase
            => GetEffect<T>()?.Intensity ?? 0;

        public byte GetIntensity(Type type)
            => GetEffect(type)?.Intensity ?? 0;

        public byte GetIntensity(string name, bool lowerCase = true)
            => GetEffect(name, lowerCase)?.Intensity ?? 0;
        #endregion

        #region SetIntensity
        public void SetIntensity<T>(byte intensity) where T : StatusEffectBase
            => GetEffect<T>()!.Intensity = intensity;

        public void SetIntensity(Type type, byte intensity)
            => GetEffect(type)!.Intensity = intensity;

        public void SetIntensity(string name, bool lowerCase, byte intensity)
            => GetEffect(name, lowerCase)!.Intensity = intensity;
        #endregion

        #region AddIntensity
        public void AddIntensity<T>(byte intensity) where T : StatusEffectBase
        {
            var effect = GetEffect<T>();

            if (effect is null)
                return;

            effect.ServerSetState((byte)Mathf.Clamp(0f, effect.Intensity + intensity, 255f));
        }

        public void AddIntensity(Type type, byte intensity)
        {
            var effect = GetEffect(type);

            if (effect is null)
                return;

            effect.ServerSetState((byte)Mathf.Clamp(0f, effect.Intensity + intensity, 255f));
        }

        public void AddIntensity(string name, bool lowerCase, byte intensity)
        {
            var effect = GetEffect(name, lowerCase);

            if (effect is null)
                return;

            effect.ServerSetState((byte)Mathf.Clamp(0f, effect.Intensity + intensity, 255f));
        }
        #endregion

        #region RemoveIntensity
        public void RemoveIntensity<T>(byte intensity) where T : StatusEffectBase
        {
            var effect = GetEffect<T>();

            if (effect is null)
                return;

            var newIntensity = effect.Intensity - intensity;

            if (newIntensity <= 0)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerSetState((byte)newIntensity);
        }

        public void RemoveIntensity(Type type, byte intensity)
        {
            var effect = GetEffect(type);

            if (effect is null)
                return;

            var newIntensity = effect.Intensity - intensity;

            if (newIntensity <= 0)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerSetState((byte)newIntensity);
        }

        public void RemoveIntensity(string name, bool lowerCase, byte intensity)
        {
            var effect = GetEffect(name, lowerCase);

            if (effect is null)
                return;

            var newIntensity = effect.Intensity - intensity;

            if (newIntensity <= 0)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerSetState((byte)newIntensity);
        }
        #endregion

        #region AddDuration
        public void AddDuration<T>(float duration) where T : StatusEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return;

            effect.ServerSetState(effect.Intensity, duration, true);
        }

        public void AddDuration(Type type, float duration)
        {
            if (!TryGetEffect(type, out var effect))
                return;

            effect.ServerSetState(effect.Intensity, duration, true);
        }

        public void AddDuration(string name, bool lowerCase, float duration)
        {
            if (!TryGetEffect(name, lowerCase, out var effect))
                return;

            effect.ServerSetState(effect.Intensity, duration, true);
        }
        #endregion

        #region RemoveDuration
        public void RemoveDuration<T>(float duration) where T : StatusEffectBase
        {
            if (!TryGetEffect<T>(out var effect))
                return;

            var newDuration = effect.Duration - duration;

            if (newDuration <= 0f)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerChangeDuration(newDuration, false);
        }

        public void RemoveDuration(Type type, float duration)
        {
            if (!TryGetEffect(type, out var effect))
                return;

            var newDuration = effect.Duration - duration;

            if (newDuration <= 0f)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerChangeDuration(newDuration, false);
        }

        public void RemoveDuration(string name, bool lowerCase, float duration)
        {
            if (!TryGetEffect(name, lowerCase, out var effect))
                return;

            var newDuration = effect.Duration - duration;

            if (newDuration <= 0f)
            {
                effect.ServerDisable();
                return;
            }

            effect.ServerChangeDuration(newDuration, false);
        }
        #endregion

        #region GetDuration
        public TimeSpan GetDuration<T>() where T : StatusEffectBase
        {
            if (!TryGetEffect<T>(out var effect) || !effect.IsEnabled || effect.Duration <= 0f)
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(effect.Duration);
        }

        public TimeSpan GetDuration(Type type)
        {
            if (!TryGetEffect(type, out var effect) || !effect.IsEnabled || effect.Duration <= 0f)
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(effect.Duration);
        }

        public TimeSpan GetDuration(string name, bool lowerCase = true)
        {
            if (!TryGetEffect(name, lowerCase, out var effect) || !effect.IsEnabled || effect.Duration <= 0f)
                return TimeSpan.Zero;

            return TimeSpan.FromSeconds(effect.Duration);
        }
        #endregion

        public IEnumerable<string> GetNames(bool lowerCase = false)
            => Effects.Select(x => lowerCase ? x.Key.Name.ToLower() : x.Key.Name);

        public IEnumerable<StatusEffectBase> GetEffects(Predicate<StatusEffectBase> predicate)
            => Effects.Where(x => predicate(x.Value)).Select(y => y.Value);

        public bool TryGetEffect(string name, bool lowerCase, out StatusEffectBase effect)
            => (effect = GetEffect(name, lowerCase)) != null;

        public bool TryGetEffect(Type type, out StatusEffectBase effect)
            => (effect = GetEffect(type)) != null;

        public StatusEffectBase GetEffect(string name, bool lowerCase = true)
            => GetEffect(x => lowerCase ? name.ToLower() == x.Key.Name.ToLower() : name == x.Key.Name);

        public StatusEffectBase GetEffect(Type type)
            => Effects.TryGetValue(type, out var effect) ? effect : null;

        public StatusEffectBase GetEffect(Predicate<KeyValuePair<Type, StatusEffectBase>> predicate)
        {
            foreach (var pair in Effects)
            {
                if (!predicate(pair))
                    continue;

                return pair.Value;
            }

            return null;
        }

        public bool TryGetEffect<T>(out T effect) where T : StatusEffectBase
            => (effect = GetEffect<T>()) != null;

        public T GetEffect<T>() where T : StatusEffectBase
        {
            if (!Effects.TryGetValue(typeof(T), out var effect))
                return null;

            return (T)effect;
        }
    }
}