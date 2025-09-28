using CustomPlayerEffects;

using CustomRendering;

using InventorySystem.Items.Usables.Scp244.Hypothermia;

using LabExtended.API.Custom.Effects;

using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;

using LabExtended.Events;
using LabExtended.Extensions;

using NorthwoodLib.Pools;

using System.Reflection;

using LabApi.Events.Arguments.PlayerEvents;

using UnityEngine;
// ReSharper disable UnusedAutoPropertyAccessor.Local

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace LabExtended.API.Containers;

/// <summary>
/// Contains all status effects.
/// </summary>
public class EffectContainer : IDisposable
{
    private static readonly IEnumerable<PropertyInfo> _properties;

    static EffectContainer()
        => _properties = typeof(EffectContainer).FindProperties(x => (x.GetSetMethod(true)?.IsPrivate ?? false));

    /// <summary>
    /// Gets the dictionary that contains all status effects.
    /// </summary>
    public Dictionary<Type, StatusEffectBase> Effects { get; internal set; }
    
    /// <summary>
    /// Gets the dictionary that contains all custom effects.
    /// </summary>
    public Dictionary<Type, CustomPlayerEffect> CustomEffects { get; internal set; }

    /// <summary>
    /// Gets the target player's effects controller.
    /// </summary>
    public PlayerEffectsController Controller { get; }

    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer Player { get; }

    /// <summary>
    /// Gets the total amount of status effects (without custom effects).
    /// </summary>
    public int Count => Effects.Count;

    /// <summary>
    /// Gets the amount of active status effects.
    /// </summary>
    public int ActiveEffectsCount => Effects.Count(x => x.Value.IsEnabled);
    
    /// <summary>
    /// Gets the amount of disabled status effects.
    /// </summary>
    public int InactiveEffectsCount => Effects.Count(x => !x.Value.IsEnabled);

    /// <summary>
    /// Gets a list of all active status effects.
    /// </summary>
    public IEnumerable<StatusEffectBase> ActiveEffects => GetEffects(x => x.IsEnabled);
    
    /// <summary>
    /// Gets a list of all disabled status effects.
    /// </summary>
    public IEnumerable<StatusEffectBase> InactiveEffects => GetEffects(x => !x.IsEnabled);

    #region Effect Properties
    /// <summary>
    /// Gets the player's AmnesiaItems effect.
    /// </summary>
    public AmnesiaItems AmnesiaItems { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public AmnesiaVision AmnesiaVision { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public AntiScp207 AntiScp207 { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Asphyxiated Asphyxiated { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Burned Burned { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Blurred Blurred { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Bleeding Bleeding { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Blindness Blindness { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public BodyshotReduction BodyshotReduction { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public CardiacArrest CardiacArrest { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Concussed Concussed { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Corroding Corroding { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public DamageReduction DamageReduction { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Decontaminating Decontaminating { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Deafened Deafened { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Disabled Disabled { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Ensnared Ensnared { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Exhausted Exhausted { get; private set; }
    
    /// <summary>
    /// Gets the player's Fade effect.
    /// </summary>
    public Fade Fade { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Flashed Flashed { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public FogControl FogControl { get; private set; }
    
    /// <summary>
    /// Gets the player's Lightweight effect.
    /// </summary>
    public Lightweight Lightweight { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Ghostly Ghostly { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Hemorrhage Hemorrhage { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Hypothermia Hypothermia { get; private set; }
    
    /// <summary>
    /// Gets the player's HeavyFooted effect.
    /// </summary>
    public HeavyFooted HeavyFooted { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public InsufficientLighting InsufficientLighting { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Invigorated Invigorated { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Invisible Invisible { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public MovementBoost MovementBoost { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public PocketCorroding PocketCorroding { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Poisoned Poisoned { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public PitDeath PitDeath { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public RainbowTaste RainbowTaste { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Scp207 Scp207 { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Scp1853 Scp1853 { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Scp1344 Scp1344 { get; private set; }
    
    /// <summary>
    /// Gets the player's SCP-1576 effect.
    /// </summary>
    public Scp1576 Scp1576 { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Scanned Scanned { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Stained Stained { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Sinkhole Sinkhole { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Slowness Slowness { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Strangled Strangled { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public SilentWalk SilentWalk { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public SeveredEyes SeveredEyes { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public SeveredHands SeveredHands { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public SoundtrackMute SoundtrackMute { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public SpawnProtected SpawnProtected { get; private set; }
    
    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Scp1344Detected Scp1344Detected { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Traumatized Traumatized { get; private set; }

    /// <summary>
    /// Gets the player's AmnesiaVision effect.
    /// </summary>
    public Vitality Vitality { get; private set; }
    #endregion

    /// <summary>
    /// Whether or not the player has a forced fog type.
    /// </summary>
    public bool HasForcedFog => FogControl.Intensity > 0;

    /// <summary>
    /// Whether or not the player's soundtrack audio loop is paused.
    /// </summary>
    public bool HasMutedSoundtrack
    {
        get => SoundtrackMute.IsEnabled;
        set => SoundtrackMute.IsEnabled = value;
    }

    /// <summary>
    /// Whether or not the player has jumping disabled by the <see cref="HeavyFooted"/> effect.
    /// </summary>
    public bool HasJumpingDisabled
    {
        get => HeavyFooted.Intensity >= 100;
        set => HeavyFooted.Intensity = value ? (byte)100 : (byte)0;
    }

    /// <summary>
    /// Gets or sets the intensity of the player's Lightweight effect.
    /// </summary>
    public byte LightweightIntensity
    {
        get => Lightweight.Intensity;
        set => Lightweight.Intensity = value;
    }

    /// <summary>
    /// Gets or sets the intensity of the player's HeavyFooted effect.
    /// </summary>
    public byte HeavyFootedIntensity
    {
        get => HeavyFooted.Intensity;
        set => HeavyFooted.Intensity = value;
    }

    /// <summary>
    /// Gets or sets the player's jump intensity - allows values from -100 to 255.
    /// <remarks>Values below zero decrease the player's jump strength (completely disabled by -100) while values above zero add 1% to the player's jump intensity (doubled at 100).</remarks>
    /// </summary>
    public int JumpIntensity
    {
        get
        {
            return Lightweight.Intensity - HeavyFooted.Intensity;
        }
        set
        {
            if (value > 0)
            {
                HeavyFooted.Intensity = 0;
                Lightweight.Intensity = (byte)Mathf.Clamp(0, value, 255);
            }
            else
            {
                Lightweight.Intensity = 0;
                HeavyFooted.Intensity = (byte)Mathf.Clamp(0, -value, 255);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current intensity of the player's Fade effect.
    /// </summary>
    public byte FadeIntensity
    {
        get => Fade.Intensity;
        set => Fade.Intensity = value;
    }

    /// <summary>
    /// Gets or sets the type of fog that is forced.
    /// </summary>
    public FogType ForcedFog
    {
        get => HasForcedFog ? (FogType)(FogControl.Intensity - 1) : FogType.None;
        set => FogControl.SetFogType(value);
    }

    internal EffectContainer(PlayerEffectsController controller, ExPlayer player)
    {
        if (controller is null)
            throw new ArgumentNullException(nameof(controller));

        try
        {
            var dict = DictionaryPool<Type, StatusEffectBase>.Shared.Rent();
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
            Player = player;
            Controller = controller;

            CustomEffects = DictionaryPool<Type, CustomPlayerEffect>.Shared.Rent();

            InternalEvents.OnRoleChanged += OnRoleChanged;

            if (props.Count != _properties.Count())
            {
                ApiLog.Warn("Effect API",
                    $"Failed to set some effect properties (total={_properties.Count()} / set={props.Count})");

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
            ApiLog.Error("Effect API",
                $"An error occurred while setting up the effect container!\n{ex.ToColoredString()}");
        }
    }

    /// <summary>
    /// Enables all status effects (by setting their intensity to one).
    /// </summary>
    public void EnableAllEffects()
        => Effects.ForEach(x => x.Value.ServerSetState(1));

    /// <summary>
    /// Disables all status effects.
    /// </summary>
    public void DisableAllEffects()
        => Effects.ForEach(x => x.Value.ServerDisable());

    /// <summary>
    /// Disables all active status effects.
    /// </summary>
    public void DisableActiveEffects()
        => ActiveEffects.ForEach(x => x.ServerDisable());

    /// <summary>
    /// Enables all disabled status effects.
    /// </summary>
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

    public void EnableEffect<T>(byte intensity, float duration = 0f, bool addDurationIfActive = false)
        where T : StatusEffectBase
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

    public bool HasCustomEffect<T>() where T : CustomPlayerEffect
        => CustomEffects.ContainsKey(typeof(T));

    public bool HasCustomEffectActive<T>() where T : CustomPlayerEffect
        => CustomEffects.TryGetValue(typeof(T), out var effect) && effect.IsActive;

    public bool TryGetCustomEffect<T>(out T effect) where T : CustomPlayerEffect
    {
        effect = null;

        if (!CustomEffects.TryGetValue(typeof(T), out var instance) || effect is not T effectInstance)
            return false;

        effect = effectInstance;
        return true;
    }

    public T GetCustomEffect<T>() where T : CustomPlayerEffect
    {
        if (!CustomEffects.TryGetValue(typeof(T), out var effect))
            throw new KeyNotFoundException($"No effect found for type {typeof(T).Name}");

        return (T)effect;
    }

    public T GetOrAddCustomEffect<T>(bool enableEffect = false) where T : CustomPlayerEffect
    {
        if (!TryGetCustomEffect<T>(out var effect))
            return AddCustomEffect<T>(enableEffect);

        return effect;
    }

    public T AddCustomEffect<T>(bool enableEffect = false) where T : CustomPlayerEffect
    {
        if (CustomEffects.TryGetValue(typeof(T), out var activeEffect))
            return (T)activeEffect;

        var effect = Activator.CreateInstance<T>();

        CustomEffects.Add(typeof(T), effect);

        effect.Player = Player;
        effect.Start();

        if (enableEffect)
            effect.Enable();

        return effect;
    }

    public bool RemoveCustomEffect<T>() where T : CustomPlayerEffect
    {
        if (!CustomEffects.TryGetValue(typeof(T), out var effect))
            return false;

        CustomEffects.Remove(typeof(T));

        if (effect.IsActive)
            effect.Disable();

        effect.Stop();
        return true;
    }

    public void RemoveCustomEffects()
    {
        foreach (var customEffect in CustomEffects)
        {
            if (customEffect.Value.IsActive)
                customEffect.Value.Disable();

            customEffect.Value.Stop();
        }

        CustomEffects.Clear();
    }

    public void Dispose()
    {
        if (Effects != null)
        {
            DictionaryPool<Type, StatusEffectBase>.Shared.Return(Effects);
            Effects = null;
        }

        if (CustomEffects != null)
        {
            foreach (var customEffect in CustomEffects)
            {
                if (customEffect.Value.IsActive)
                    customEffect.Value.Disable();

                customEffect.Value.Stop();
            }

            DictionaryPool<Type, CustomPlayerEffect>.Shared.Return(CustomEffects);
            CustomEffects = null;
        }

        InternalEvents.OnRoleChanged -= OnRoleChanged;
    }

    private void OnRoleChanged(PlayerChangedRoleEventArgs args)
    {
        if (!Player || args.Player != Player)
            return;

        foreach (var effect in CustomEffects)
        {
            if (!effect.Value.IsActive)
                continue;

            if (!effect.Value.OnRoleChanged(args.NewRole.RoleTypeId))
                effect.Value.Disable();
        }
    }
}