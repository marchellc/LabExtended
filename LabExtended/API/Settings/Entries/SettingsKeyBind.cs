using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UnityEngine;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsKeyBind : SettingsEntry, IWrapper<SSKeybindSetting>
    {
        private bool _isPressed;

        public SettingsKeyBind(
            string customId, 
            string settingLabel, 
            
            KeyCode suggestedKey = KeyCode.None,
            bool shouldPreventOnGuiInteraction = true, 
            string settingHint = null)
            : base(new SSKeybindSetting(
                    SettingsManager.GetIntegerId(customId),

                    settingLabel,
                    suggestedKey,

                    shouldPreventOnGuiInteraction,
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

        public new SSKeybindSetting Base { get; }
        
        public Action<SettingsKeyBind> OnPressed { get; set; }

        public float? LastPressTime { get; private set; }

        public TimeSpan? TimeSinceLastPress
        {
            get
            {
                if (!LastPressTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastPressTime.Value);
            }
        }
        
        public KeyCode AssignedKey => Base.AssignedKeyCode;

        public bool IsPressed => Base.SyncIsPressed;
        public bool WasPressed => _isPressed;

        public KeyCode SuggestedKey
        {
            get => Base.SuggestedKey;
            set => Base.SuggestedKey = value;
        }

        public bool ShouldPreventOnGuiInteraction
        {
            get => Base.PreventInteractionOnGUI;
            set => Base.PreventInteractionOnGUI = value;
        }

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();

            if (_isPressed == IsPressed)
                return;

            _isPressed = IsPressed;

            LastPressTime = Time.realtimeSinceStartup;

            HandlePress(IsPressed);
            
            OnPressed.InvokeSafe(this);
        }
        
        public virtual void HandlePress(bool isPressed) { }

        public override string ToString()
            => $"SettingsKeyBind (CustomId={CustomId}; AssignedId={AssignedId}; SuggestedKey={SuggestedKey}; Ply={Player?.UserId ?? "null"})";

        public static SettingsKeyBind Create(string customId, string settingLabel, KeyCode suggestedKey = KeyCode.None, bool shouldPreventOnGuiInteraction = true, string settingHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(settingLabel))
                throw new ArgumentNullException(nameof(settingLabel));

            var keybindId = SettingsManager.GetIntegerId(customId);
            var keybind = new SSKeybindSetting(keybindId, settingLabel, suggestedKey, shouldPreventOnGuiInteraction, settingHint);

            return new SettingsKeyBind(keybind, customId);
        }
    }
}