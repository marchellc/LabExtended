using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsPlainText : SettingsEntry, IWrapper<SSPlaintextSetting>
    {
        public SettingsPlainText(SSPlaintextSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSPlaintextSetting Base { get; }
        
        public Action<SettingsPlainText> OnUpdated { get; set; }

        public string Text => Base.SyncInputText;

        public int CharacterLimit
        {
            get => Base.CharacterLimit;
            set => Base.CharacterLimit = value;
        }

        public string PlaceHolder
        {
            get => Base.Placeholder;
            set => Base.Placeholder = value;
        }

        public TMP_InputField.ContentType ContentType
        {
            get => Base.ContentType;
            set => Base.ContentType = value;
        }

        public void Clear()
            => Player?.Connection?.Send(new SSSUpdateMessage(Base, null));

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();
            OnUpdated.InvokeSafe(this);
        }

        public static SettingsPlainText Create(string customId, string settingLabel, string placeHolder = "...", int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string settingHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(settingLabel))
                throw new ArgumentNullException(nameof(settingLabel));

            var settingId = SettingsManager.GetIntegerId(customId);
            var setting = new SSPlaintextSetting(settingId, settingLabel, placeHolder, characterLimit, contentType, settingHint);

            return new SettingsPlainText(setting, customId);
        }
    }
}