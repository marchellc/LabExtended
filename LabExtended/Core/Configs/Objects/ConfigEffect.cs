using CustomPlayerEffects;

using LabExtended.API;
using LabExtended.API.Custom.Effects;

using LabExtended.Extensions;
using LabExtended.Utilities;

using System.ComponentModel;

using YamlDotNet.Serialization;

namespace LabExtended.Core.Configs.Objects
{
    /// <summary>
    /// Represents the configuration for a status effect, including its name, intensity, duration, and type information
    /// for both base-game and custom effects.
    /// </summary>
    public class ConfigEffect
    {
        static ConfigEffect()
        {
            var list = new List<Type>();

            foreach (var type in ReflectionUtils.GameAssembly.GetTypes())
            {
                if (typeof(StatusEffectBase).IsAssignableFrom(type))
                {
                    list.Add(type);
                }
            }

            BaseEffects = list.AsReadOnly();
        }

        /// <summary>
        /// Gets the collection of base-game effect types.
        /// </summary>
        public static IReadOnlyList<Type> BaseEffects { get; }

        private Type baseEffect;
        private Type customEffect;

        private bool hasChecked;

        /// <summary>
        /// Gets or sets the name of the effect.
        /// </summary>
        [Description("Sets the name of the effect.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the intensity level of the effect.
        /// </summary>
        [Description("Sets the intensity of the effect.")]
        public byte? Intensity { get; set; }

        /// <summary>
        /// Gets or sets the duration of the effect, in seconds.
        /// </summary>
        [Description("Sets the duration of the effect.")]
        public float? Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the effect's duration is extended when the effect is already active.
        /// </summary>
        [Description("Whether or not the effect's duration should add if it's already active.")]
        public bool StackEffect { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not this is a base-game effect.
        /// </summary>
        [YamlIgnore]
        public bool IsBase
        {
            get
            {
                TryGet();
                return baseEffect != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the effect is currently valid and can be used.
        /// </summary>
        [YamlIgnore]
        public bool IsValid
        {
            get
            {
                hasChecked = false;

                TryGet();

                return baseEffect != null || customEffect != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not this is a custom effect.
        /// </summary>
        [YamlIgnore]
        public bool IsCustom
        {
            get
            {
                if (this.customEffect == null)
                {
                    if (CustomPlayerEffect.Effects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var customEffect))
                    {
                        this.customEffect = customEffect;
                    }
                }

                return this.customEffect != null;
            }
        }

        /// <summary>
        /// Gets the type of the base-game effect.
        /// </summary>
        public Type BaseType
        {
            get
            {
                TryGet();
                return baseEffect;
            }
        }

        /// <summary>
        /// Gets the type of the custom effect.
        /// </summary>
        public Type? CustomType
        {
            get
            {
                if (this.customEffect == null)
                {
                    if (CustomPlayerEffect.Effects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var customEffect))
                    {
                        this.customEffect = customEffect;
                    }
                }

                return this.customEffect;
            }
        }

        /// <summary>
        /// Attempts to retrieve the first status effect of the specified base type from the given reference hub.
        /// </summary>
        /// <remarks>If <c>BaseType</c> is null, the method returns false and <paramref
        /// name="statusEffect"/> is set to null.</remarks>
        /// <param name="hub">The reference hub from which to search for the status effect. Cannot be null.</param>
        /// <param name="statusEffect">When this method returns, contains the first matching status effect if found; otherwise, null.</param>
        /// <returns>true if a status effect of the specified base type is found; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="hub"/> is null.</exception>
        public bool TryGetBase(ReferenceHub hub, out StatusEffectBase statusEffect)
        {
            if (hub == null)
                throw new ArgumentNullException(nameof(hub));

            statusEffect = null!;

            if (BaseType == null)
                return false;

            return hub.playerEffectsController.AllEffects.TryGetFirst(x => x.GetType() == BaseType, out statusEffect);
        }

        /// <summary>
        /// Attempts to retrieve the custom player effect associated with the specified player.
        /// </summary>
        /// <remarks>This method does not modify the player's effects. It only attempts to retrieve an
        /// existing custom effect associated with the player. If the effect is not present, the method returns false
        /// and customPlayerEffect is set to null.</remarks>
        /// <param name="player">The player for whom to retrieve the custom effect. Cannot be null, and must have a valid ReferenceHub.</param>
        /// <param name="customPlayerEffect">When this method returns, contains the custom player effect associated with the player, if found; otherwise,
        /// null.</param>
        /// <returns>true if the custom player effect was found and retrieved; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the player parameter is null or does not have a valid ReferenceHub.</exception>
        public bool TryGetCustom(ExPlayer player, out CustomPlayerEffect customPlayerEffect)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(player));

            customPlayerEffect = null!;

            if (hasChecked && this.customEffect == null)
            {
                if (CustomPlayerEffect.Effects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var customEffect))
                {
                    this.customEffect = customEffect;
                }
            }
            else
            {
                TryGet();
            }

            if (this.customEffect == null)
                return false;

            return player.Effects.CustomEffects.TryGetValue(this.customEffect, out customPlayerEffect);
        }

        /// <summary>
        /// Attempts to apply the configured effect to the specified player. Returns a value indicating whether the
        /// effect was successfully enabled.
        /// </summary>
        /// <remarks>If the effect configuration is invalid or the effect cannot be found, the method
        /// returns false and logs a warning. The method supports both base and custom effects, applying the appropriate
        /// effect type based on the configuration.</remarks>
        /// <param name="player">The player to whom the effect will be applied. Cannot be null and must have a valid ReferenceHub.</param>
        /// <returns>true if the effect was successfully enabled on the player; otherwise, false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the player parameter is null or does not have a valid ReferenceHub.</exception>
        public bool Apply(ExPlayer player)
        {
            if (player?.ReferenceHub == null)
                throw new ArgumentNullException(nameof(player));

            if (hasChecked && this.customEffect == null)
            {
                if (CustomPlayerEffect.Effects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var customEffect))
                {
                    this.customEffect = customEffect;
                }
            }
            else
            {
                TryGet();
            }

            if (baseEffect != null)
            {
                player.Effects.EnableEffect(baseEffect, Intensity ?? 0, Duration ?? 0f, StackEffect);
                return true;
            }
            else if (customEffect != null)
            {
                if (!player.Effects.CustomEffects.TryGetValue(customEffect, out var customInstance))
                {
                    ApiLog.Warn("LabExtended", $"Effect &3{customEffect.FullName}&r could not be found in dictionary of player {player.ToLogString()}");
                    return false;
                }

                customInstance.Enable();

                if (Duration.HasValue && customInstance is CustomDurationEffect customDurationEffect)
                    customDurationEffect.RemainingDuration = Duration.Value;

                return customInstance.IsActive;
            }
            else
            {
                ApiLog.Warn("LabExtended", $"Tried to apply an invalid config effect (&3{Name}&r)!");
                return false;
            }
        }

        private void TryGet()
        {
            if (hasChecked)
                return;

            hasChecked = true;

            if (Name?.Length < 1)
                return;

            if (CustomPlayerEffect.Effects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var customEffect))
                this.customEffect = customEffect;
            else if (BaseEffects.TryGetFirst(x => string.Equals(x.Name, Name, StringComparison.OrdinalIgnoreCase), out var baseEffect))
                this.baseEffect = baseEffect;
            else
                ApiLog.Warn("LabExtended", $"Effect &3{Name}&r could not be found in config.");
        }
    }
}