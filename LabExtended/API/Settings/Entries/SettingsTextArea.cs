using LabExtended.API.Interfaces;
using LabExtended.Extensions;
using Mirror;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsTextArea : SettingsEntry, IWrapper<SSTextArea>
    {
        public SettingsTextArea(
            string customId,
            string settingsText,
            string collapsedText,

            TextAlignmentOptions alignmentOptions = TextAlignmentOptions.TopLeft,
            SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable)

            : base(new SSTextArea(
                    SettingsManager.GetIntegerId(customId),

                    settingsText,
                    foldoutMode,
                    collapsedText,
                    alignmentOptions),

                customId)
        {
            Base = (SSTextArea)base.Base;
        }

        private SettingsTextArea(SSTextArea baseValue, string customId) : base(baseValue, customId)
            => Base = baseValue;
        
        public new SSTextArea Base { get; }

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

        public override string ToString()
            => $"SettingsTextArea (CustomId={CustomId}; AssignedId={AssignedId}; Text={Text}; Ply={Player?.UserId ?? "null"})";

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
