using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UnityEngine;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    /// <summary>
    /// Represents a configurable key binding setting that can be assigned to a keyboard key and tracked for press
    /// events.
    /// </summary>
    public class SettingsKeyBind : SettingsEntry, IWrapper<SSKeybindSetting>
    {
        private bool _isPressed;

        /// <summary>
        /// Initializes a new instance of the SettingsKeyBind class with the specified custom identifier, label, and key
        /// binding options.
        /// </summary>
        /// <param name="customId">A unique string identifier for the key binding setting. Cannot be null or empty.</param>
        /// <param name="settingLabel">The display label for the key binding setting, shown in the user interface. Cannot be null or empty.</param>
        /// <param name="suggestedKey">The default key to suggest for this binding. Use KeyCode.None if no default is desired.</param>
        /// <param name="shouldPreventOnGuiInteraction">true to prevent the key binding from triggering while interacting with GUI elements; otherwise, false.</param>
        /// <param name="allowSpectatorTrigger">true to allow the key binding to be triggered while in spectator mode; otherwise, false.</param>
        /// <param name="settingHint">An optional hint or description for the setting, displayed as additional information to the user. Can be
        /// null.</param>
        public SettingsKeyBind(
            string customId, 
            string settingLabel, 
            
            KeyCode suggestedKey = KeyCode.None,

            bool shouldPreventOnGuiInteraction = true, 
            bool allowSpectatorTrigger = false,

            string? settingHint = null)
            : base(new SSKeybindSetting(
                    SettingsManager.GetIntegerId(customId),

                    settingLabel,
                    suggestedKey,

                    shouldPreventOnGuiInteraction,
                    allowSpectatorTrigger,
                    
                    settingHint),

                customId)
        {
            Base = (SSKeybindSetting)base.Base;

            _isPressed = Base.SyncIsPressed;
        }
        
        private SettingsKeyBind(SSKeybindSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;

            _isPressed = Base.SyncIsPressed;
        }

        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSKeybindSetting Base { get; }
        
        /// <summary>
        /// Gets or sets the delegate to invoke once the key is pressed.
        /// </summary>
        public Action<SettingsKeyBind> OnPressed { get; set; }

        /// <summary>
        /// Gets or sets the last time the key was pressed, in milliseconds since the game started (using <see cref="Time.realtimeSinceStartup"/>)
        /// </summary>
        public float? LastPressTime { get; private set; }

        /// <summary>
        /// Gets or sets the last time the key was pressed since the game started (using <see cref="Time.realtimeSinceStartup"/>)
        /// </summary>
        public TimeSpan? TimeSinceLastPress
        {
            get
            {
                if (!LastPressTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastPressTime.Value);
            }
        }
        
        /// <summary>
        /// Gets the key assigned to the keybind.
        /// </summary>
        public KeyCode AssignedKey => Base.AssignedKeyCode;

        /// <summary>
        /// Whether or not the key is currently pressed.
        /// </summary>
        public bool IsPressed => Base.SyncIsPressed;

        /// <summary>
        /// Whether or not the key was pressed.
        /// </summary>
        public bool WasPressed => _isPressed;

        /// <summary>
        /// Gets or sets the key suggested to use for the player.
        /// </summary>
        public KeyCode SuggestedKey
        {
            get => Base.SuggestedKey;
            set => Base.SuggestedKey = value;
        }

        /// <summary>
        /// Whether or not the keybind should trigger when the player is interacting with GUI elements.
        /// </summary>
        public bool ShouldPreventOnGuiInteraction
        {
            get => Base.PreventInteractionOnGUI;
            set => Base.PreventInteractionOnGUI = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether spectator-triggered actions are allowed.
        /// </summary>
        public bool AllowSpectatorTrigger
        {
            get => Base.AllowSpectatorTrigger;
            set => Base.AllowSpectatorTrigger = value;
        }

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

            if (_isPressed == IsPressed)
                return;

            _isPressed = IsPressed;

            LastPressTime = Time.realtimeSinceStartup;

            HandlePress(IsPressed);
            
            OnPressed.InvokeSafe(this);
        }

        /// <summary>
        /// An overridable method called when the key is pressed or released.
        /// </summary>
        /// <param name="isPressed">Whether or not the key is pressed.</param>
        public virtual void HandlePress(bool isPressed) { }

        /// <summary>
        /// Returns a string that represents the current object, including key binding and player information.
        /// </summary>
        /// <returns>A string containing the values of CustomId, AssignedId, SuggestedKey, and the associated player's user ID,
        /// or "null" if no player is assigned.</returns>
        public override string ToString()
            => $"SettingsKeyBind (CustomId={CustomId}; AssignedId={AssignedId}; SuggestedKey={SuggestedKey}; Ply={Player?.UserId ?? "null"})";

        /// <summary>
        /// Creates a new key binding setting with the specified identifier, label, and configuration options.
        /// </summary>
        /// <param name="customId">A unique string identifier for the key binding. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <param name="settingLabel">The display label for the key binding setting. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <param name="suggestedKey">The default key to suggest for this binding. Use KeyCode.None if no suggestion is provided.</param>
        /// <param name="shouldPreventOnGuiInteraction">true to prevent the key binding from triggering when interacting with GUI elements; otherwise, false.</param>
        /// <param name="allowSpectatorTrigger">true to allow the key binding to be triggered while in spectator mode; otherwise, false.</param>
        /// <param name="settingHint">An optional hint or description to display for the key binding. Can be null.</param>
        /// <returns>A new instance of SettingsKeyBind configured with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId or settingLabel is null, empty, or consists only of white-space characters.</exception>
        public static SettingsKeyBind Create(string customId, string settingLabel, KeyCode suggestedKey = KeyCode.None, 
            bool shouldPreventOnGuiInteraction = true, bool allowSpectatorTrigger = false, string? settingHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(settingLabel))
                throw new ArgumentNullException(nameof(settingLabel));

            var keybindId = SettingsManager.GetIntegerId(customId);
            var keybind = new SSKeybindSetting(keybindId, settingLabel, suggestedKey, shouldPreventOnGuiInteraction, allowSpectatorTrigger, settingHint);

            return new SettingsKeyBind(keybind, customId);
        }
    }
}