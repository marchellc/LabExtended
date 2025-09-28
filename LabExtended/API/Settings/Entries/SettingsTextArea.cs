using LabExtended.API.Interfaces;

using Mirror;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    /// <summary>
    /// Represents a settings entry that displays a multi-line text area, allowing users to view text within a
    /// settings interface.
    /// </summary>
    public class SettingsTextArea : SettingsEntry, IWrapper<SSTextArea>
    {
        /// <summary>
        /// Initializes a new instance of the SettingsTextArea class with the specified identifier, display text,
        /// collapsed text, alignment options, and foldout mode.
        /// </summary>
        /// <param name="customId">A unique string identifier for the settings text area. Used to associate the control with a specific
        /// settings entry.</param>
        /// <param name="settingsText">The text to display within the settings text area when expanded.</param>
        /// <param name="collapsedText">The text to display when the settings text area is collapsed. This is shown if the foldout mode allows
        /// collapsing.</param>
        /// <param name="alignmentOptions">Specifies the alignment of the text within the area. The default is TopLeft.</param>
        /// <param name="foldoutMode">Determines whether the text area can be collapsed and, if so, how the foldout behavior operates. The default
        /// is NotCollapsable.</param>
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
        
        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSTextArea Base { get; }

        /// <summary>
        /// Gets or sets alignment options for the text within the text area.
        /// </summary>
        public TextAlignmentOptions AlignmentOptions
        {
            get => Base.AlignmentOptions;
            set => Base.AlignmentOptions = value;
        }

        /// <summary>
        /// Gets or sets fouldout mode.
        /// </summary>
        public SSTextArea.FoldoutMode FoldoutMode
        {
            get => Base.Foldout;
            set => Base.Foldout = value;
        }

        /// <summary>
        /// Gets or sets the text that is shown.
        /// </summary>
        public string Text
        {
            get => Base.Label;
            set
            {
                Base.Label = value;
                SendText(value);
            }
        }
        
        /// <summary>
        /// Sends a text update.
        /// </summary>
        /// <param name="text">The text to set.</param>
        public void SendText(string text)
            => Player?.Connection?.Send(new SSSUpdateMessage(Base, writer => writer.WriteString(text)));

        /// <summary>
        /// Returns a string that represents the current state of the SettingsTextArea instance.
        /// </summary>
        /// <returns>A string containing the values of the CustomId, AssignedId, Text, and Player.UserId properties.</returns>
        public override string ToString()
            => $"SettingsTextArea (CustomId={CustomId}; AssignedId={AssignedId}; Text={Text}; Ply={Player?.UserId ?? "null"})";

        /// <summary>
        /// Creates a new instance of the SettingsTextArea control with the specified identifier, display text,
        /// collapsed text, alignment, and foldout mode.
        /// </summary>
        /// <param name="customId">A unique string identifier for the settings text area. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <param name="settingsText">The text to display within the settings area. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <param name="collapsedText">The text to display when the settings area is collapsed. This value is used only if the foldout mode allows
        /// collapsing.</param>
        /// <param name="alignmentOptions">Specifies the text alignment within the settings area. The default is TextAlignmentOptions.TopLeft.</param>
        /// <param name="foldoutMode">Determines whether the settings area can be collapsed and how the foldout behavior is handled. The default
        /// is FoldoutMode.NotCollapsable.</param>
        /// <returns>A new SettingsTextArea instance configured with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId or settingsText is null, empty, or consists only of white-space characters.</exception>
        public static SettingsTextArea Create(string customId, string settingsText, string collapsedText, 
            TextAlignmentOptions alignmentOptions = TextAlignmentOptions.TopLeft, SSTextArea.FoldoutMode foldoutMode = SSTextArea.FoldoutMode.NotCollapsable)
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
