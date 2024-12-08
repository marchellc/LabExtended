using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UnityEngine;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsKeyBind : SettingsEntry, IWrapper<SSKeybindSetting>
    {
        public SettingsKeyBind(SSKeybindSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSKeybindSetting Base { get; }
        
        public Action<SettingsKeyBind> OnPressed { get; set; }

        public KeyCode AssignedKey => Base.AssignedKeyCode;

        public bool IsPressed => Base.SyncIsPressed;

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
            OnPressed.InvokeSafe(this);
        }

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