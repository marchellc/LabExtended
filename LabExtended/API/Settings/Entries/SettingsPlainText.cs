using LabExtended.API.Interfaces;
using LabExtended.API.Settings.Interfaces;

using LabExtended.Extensions;

using Mirror;

using TMPro;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    /// <summary>
    /// Represents a plain text user-editable settings entry with configurable character limit, placeholder, and content
    /// type. Provides access to the current and previous text values, and supports custom update handling.
    /// </summary>
    /// <remarks>This class is typically used to expose a single-line or multi-line text input field as part
    /// of a settings UI. It allows customization of input constraints and notifies listeners when the text value
    /// changes. The class is not thread-safe and should be accessed from the main thread only. Use the <see
    /// cref="OnUpdated"/> event to respond to user input changes.</remarks>
    public class SettingsPlainText : SettingsEntry, 
                                     IWrapper<SSPlaintextSetting>,
                                     ICustomReaderSetting
    {
        private string _prevText;

        /// <summary>
        /// Initializes a new instance of the SettingsPlainText class with the specified identifier, label, placeholder
        /// text, character limit, content type, and optional hint.
        /// </summary>
        /// <param name="customId">A unique string identifier for the setting. Used to associate the setting with a specific configuration
        /// entry. Cannot be null or empty.</param>
        /// <param name="settingLabel">The display label for the setting, shown to users in the UI. Cannot be null or empty.</param>
        /// <param name="placeHolder">The placeholder text displayed in the input field when it is empty. Defaults to "..." if not specified.</param>
        /// <param name="characterLimit">The maximum number of characters allowed in the input field. Must be a positive integer. Defaults to 64.</param>
        /// <param name="contentType">The type of content allowed in the input field, such as standard text, password, or numeric input. Defaults
        /// to TMP_InputField.ContentType.Standard.</param>
        /// <param name="settingHint">An optional hint or description to assist the user in understanding the setting. Can be null if no hint is
        /// needed.</param>
        public SettingsPlainText(
            string customId,
            string settingLabel,
            string placeHolder = "...",

            int characterLimit = 64,
            TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
            string? settingHint = null) : base(
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

        /// <summary>
        /// Gets or sets the callback that is invoked when the settings are updated.
        /// </summary>
        public Action<SettingsPlainText> OnUpdated { get; set; }
        
        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSPlaintextSetting Base { get; }
        
        /// <summary>
        /// Gets the current text of the field.
        /// </summary>
        public string Text => Base.SyncInputText;

        /// <summary>
        /// Gets the previous text of the field.
        /// </summary>
        public string PreviousText => _prevText;

        /// <summary>
        /// Gets or sets the character limit of the field.
        /// </summary>
        public int CharacterLimit
        {
            get => Base.CharacterLimit;
            set => Base.CharacterLimit = value;
        }

        /// <summary>
        /// Gets or sets the field placeholder.
        /// </summary>
        public string PlaceHolder
        {
            get => Base.Placeholder;
            set => Base.Placeholder = value;
        }

        /// <summary>
        /// Gets or sets the content type of the field.
        /// </summary>
        public TMP_InputField.ContentType ContentType
        {
            get => Base.ContentType;
            set => Base.ContentType = value;
        }

        /// <summary>
        /// Clears the text field.
        /// </summary>
        public void Clear()
            => Player?.Connection?.Send(new SSSUpdateMessage(Base, null));

        /// <summary>
        /// Reads input text from the specified network reader and updates the synchronized input text, applying the
        /// character limit if necessary.
        /// </summary>
        /// <remarks>If the input text is null, empty, or consists only of whitespace, the synchronized
        /// input text is set to an empty string. If the input text exceeds the character limit, it is truncated to the
        /// maximum allowed length.</remarks>
        /// <param name="reader">The network reader from which to read the input text. Cannot be null.</param>
        public void Read(NetworkReader reader)
        {
            var text = reader.ReadString();

            if (string.IsNullOrWhiteSpace(text))
            {
                Base.SyncInputText = string.Empty;
                return;
            }
            
            if (text.Length > CharacterLimit)
                text = text.Substring(0, CharacterLimit);
            
            Base.SyncInputText = text;
        }

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

            if (_prevText != null && _prevText == Text)
                return;

            _prevText = Text;

            HandleInput(Text);
            
            OnUpdated.InvokeSafe(this);
        }

        /// <summary>
        /// An overridable method called when the field's text is updated. The new text is passed as a parameter.
        /// </summary>
        /// <param name="newText">The new text.</param>
        public virtual void HandleInput(string newText) { }

        /// <summary>
        /// Returns a string that represents the current object, including key property values for identification and
        /// debugging purposes.
        /// </summary>
        /// <returns>A string containing the values of CustomId, AssignedId, Text, and the UserId of the associated Player, or
        /// "null" if Player is not set.</returns>
        public override string ToString()
            => $"SettingsPlainText (CustomId={CustomId}; AssignedId={AssignedId}; Text={Text}; Ply={Player?.UserId ?? "null"})";
        
        /// <summary>
        /// Creates a new instance of the SettingsPlainText class with the specified configuration for a plain text
        /// setting.
        /// </summary>
        /// <param name="customId">A unique identifier for the setting. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="settingLabel">The display label for the setting. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="placeHolder">The placeholder text to display when the input field is empty. Defaults to "...".</param>
        /// <param name="characterLimit">The maximum number of characters allowed in the input field. Must be a positive integer. Defaults to 64.</param>
        /// <param name="contentType">The type of content allowed in the input field, such as standard text, integer, or password. Defaults to
        /// standard text.</param>
        /// <param name="settingHint">An optional hint or description to assist the user. Can be null.</param>
        /// <returns>A new SettingsPlainText instance configured with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId or settingLabel is null, empty, or consists only of white-space characters.</exception>
        public static SettingsPlainText Create(string customId, string settingLabel, string placeHolder = "...", 
            int characterLimit = 64, TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, string? settingHint = null)
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