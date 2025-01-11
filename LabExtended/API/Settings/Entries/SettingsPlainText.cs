using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsPlainText : SettingsEntry, IWrapper<SSPlaintextSetting>
    {
        private string _prevText;

        public SettingsPlainText(
            string customId,
            string settingLabel,
            string placeHolder = "...",

            int characterLimit = 64,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            string settingHint = null) : base(
            new SSPlaintextSetting(
                SettingsManager.GetIntegerId(customId),

                settingLabel,
                placeHolder,
                characterLimit,
                contentType,
                settingHint),

            customId)
        {
            Base = (SSPlaintextSetting)base.Base;
            _prevText = Base.Placeholder;
        }
        
        private SettingsPlainText(SSPlaintextSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
            _prevText = Base.Placeholder;
        }

        public Action<SettingsPlainText> OnUpdated { get; set; }
        
        public new SSPlaintextSetting Base { get; }
        
        public string Text => Base.SyncInputText;
        public string PreviousText => _prevText;

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

            if (_prevText != null && _prevText == Text)
                return;

            _prevText = Text;

            HandleInput(Text);
            
            OnUpdated.InvokeSafe(this);
        }
        
        public virtual void HandleInput(string newText) { }

        public override string ToString()
            => $"SettingsPlainText (CustomId={CustomId}; AssignedId={AssignedId}; Text={Text}; Ply={Player?.UserId ?? "null"})";
        
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