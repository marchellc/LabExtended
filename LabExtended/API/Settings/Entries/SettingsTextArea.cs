using LabExtended.API.Interfaces;
using LabExtended.Extensions;
using Mirror;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsTextArea : SettingsEntry, IWrapper<SSTextArea>
    {
        public SettingsTextArea(SSTextArea baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSTextArea Base { get; }
        
        public Action<SettingsTextArea> OnInput { get; set; }

        public TextAlignmentOptions AlignmentOptions
        {
            get => Base.AlignmentOptions;
            set => Base.AlignmentOptions = value;
        }

        public SSTextArea.FoldoutMode FoldoutMode
        {
            get => Base.Foldout;
            set => Base.Foldout = value;
        }

        public string Text
        {
            get => Base.Label;
            set
            {
                Base.Label = value;
                SendText(value);
            }
        }
        
        public void SendText(string text)
            => Player?.Connection?.Send(new SSSUpdateMessage(Base, writer => writer.WriteString(text)));

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();
            OnInput.InvokeSafe(this);
        }

        public static SettingsTextArea Create(string customId, string settingsText, string collapsedText, TextAlignmentOptions alignmentOptions = TextAlignmentOptions.TopLeft, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(settingsText))
                throw new ArgumentNullException(nameof(settingsText));

            var settingId = SettingsManager.GetIntegerId(customId);
            var setting = new SSTextArea(settingId, settingsText, foldoutMode, collapsedText, alignmentOptions);

            return new SettingsTextArea(setting, customId);
        }
    }
}
