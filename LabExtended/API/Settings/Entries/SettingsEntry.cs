using LabExtended.API.Settings.Menus;
using LabExtended.API.Wrappers;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    /// <summary>
    /// Represents the base class for wrapped setting entries.
    /// </summary>
    public class SettingsEntry : Wrapper<ServerSpecificSettingBase>
    {
        /// <summary>
        /// Initializes a new instance of the SettingsEntry class with the specified base setting and custom identifier.
        /// </summary>
        /// <param name="baseValue">The base server-specific setting to associate with this entry. Cannot be null.</param>
        /// <param name="customId">A custom identifier that uniquely distinguishes this settings entry. Cannot be null or empty.</param>
        public SettingsEntry(ServerSpecificSettingBase baseValue, string customId) : base(baseValue)
            => CustomId = customId;

        /// <summary>
        /// Gets the player that this entry belongs to.
        /// </summary>
        public ExPlayer Player { get; internal set; }

        /// <summary>
        /// Gets the menu that this entry belongs to.
        /// </summary>
        public SettingsMenu? Menu { get; internal set; }

        /// <summary>
        /// Gets the custom ID of the entry.
        /// </summary>
        public string CustomId { get; }
        
        /// <summary>
        /// Hides or shows the entry.
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets the ID assigned to the entry.
        /// </summary>
        public int AssignedId
        {
            get => Base.SettingId;
            set => Base.SettingId = value;
        }

        /// <summary>
        /// Gets or sets the label of the entry.
        /// </summary>
        public string Label
        {
            get => Base.Label;
            set => Base.Label = value;
        }

        internal virtual void Internal_Updated() { }
    }
}