namespace LabExtended.API.Settings.Entries.Dropdown
{
    public class SettingsDropdownOption
    {
        public string Text { get; }
        public object Data { get; }

        public SettingsDropdownOption(object optionData, string optionLabel)
        {
            Text = optionLabel;
            Data = optionData;
        }
    }

    public class SettingsDropdownOption<T> : SettingsDropdownOption
    {
        public SettingsDropdownOption(T optionData, string optionLabel) : base(optionData, optionLabel)
        {
            CastData = optionData;
        }

        public T CastData { get; }
    }
}