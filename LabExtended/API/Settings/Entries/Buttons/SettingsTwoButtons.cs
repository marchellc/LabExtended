using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    public class SettingsTwoButtons : SettingsEntry, IWrapper<SSTwoButtonsSetting>
    {
        public SettingsTwoButtons(SSTwoButtonsSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSTwoButtonsSetting Base { get; }
        
        public Action<SettingsTwoButtons> OnTriggered { get; set; }

        public string ButtonAText
        {
            get => Base.OptionA;
            set => Base.OptionA = value;
        }

        public string ButtonBText
        {
            get => Base.OptionB;
            set => Base.OptionB = value;
        }

        public bool IsDefaultButtonB
        {
            get => Base.DefaultIsB;
            set => Base.DefaultIsB = value;
        }

        public bool IsAButtonActive => Base.SyncIsA;
        public bool IsBButtonActive => Base.SyncIsB;

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();
            OnTriggered.InvokeSafe(this);
        }

        public static SettingsTwoButtons Create(string customId, string buttonLabel, string buttonAText, string buttonBText, bool isDefaultButtonB = true, string buttonsHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(buttonLabel))
                throw new ArgumentNullException(nameof(buttonLabel));

            if (string.IsNullOrWhiteSpace(buttonAText))
                throw new ArgumentNullException(nameof(buttonAText));

            if (string.IsNullOrWhiteSpace(buttonBText))
                throw new ArgumentNullException(nameof(buttonBText));

            var buttonId = SettingsManager.GetIntegerId(customId);
            var button = new SSTwoButtonsSetting(buttonId, buttonLabel, buttonAText, buttonBText, isDefaultButtonB, buttonsHint);

            return new SettingsTwoButtons(button, customId);
        }
    }
}