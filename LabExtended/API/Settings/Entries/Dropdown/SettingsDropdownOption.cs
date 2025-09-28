namespace LabExtended.API.Settings.Entries.Dropdown
{
    /// <summary>
    /// Represents the option in a dropdown.
    /// </summary>
    public class SettingsDropdownOption
    {
        /// <summary>
        /// The text label of the option.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// The data associated with the option.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsDropdownOption class with the specified data and display label.
        /// </summary>
        /// <param name="optionData">The data object associated with this dropdown option. This can be any value that represents the option's
        /// underlying data.</param>
        /// <param name="optionLabel">The text label to display for this option in the dropdown.</param>
        public SettingsDropdownOption(object optionData, string optionLabel)
        {
            Text = optionLabel;
            Data = optionData;
        }
    }

    /// <summary>
    /// Represents the option in a dropdown with a specific data type.
    /// </summary>
    /// <typeparam name="T">The data type.</typeparam>
    public class SettingsDropdownOption<T> : SettingsDropdownOption
    {
        /// <summary>
        /// Initializes a new instance of the SettingsDropdownOption class with the specified option data and label.
        /// </summary>
        /// <param name="optionData">The data object associated with this dropdown option. This value is used to represent the option's
        /// underlying value.</param>
        /// <param name="optionLabel">The display label for the dropdown option. This text is shown to users in the dropdown list.</param>
        public SettingsDropdownOption(T optionData, string optionLabel) : base(optionData!, optionLabel)
        {
            CastData = optionData;
        }

        /// <summary>
        /// Gets the data associated with the cast operation.
        /// </summary>
        public T CastData { get; }
    }
}