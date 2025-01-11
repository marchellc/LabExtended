using LabExtended.API.Collections.Locked;
using LabExtended.API.Interfaces;

using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Dropdown
{
    public class SettingsDropdown : SettingsEntry, IWrapper<SSDropdownSetting>
    {
        private int _prevSelectedIndex;

        public SettingsDropdown(
            string customId, 
            string dropdownLabel, 
            
            int defaultOptionIndex, 
            SSDropdownSetting.DropdownEntryType dropdownEntryType, 
            Action<SettingsDropdown> dropdownBuilder = null, 
            string dropdownHint = null)
            : base(new SSDropdownSetting(
                    SettingsManager.GetIntegerId(customId),

                    dropdownLabel,
                    null,
                    defaultOptionIndex,
                    dropdownEntryType,
                    dropdownHint),

                customId)
        {
            Base = (SSDropdownSetting)base.Base;
            Base.Options = Options.Select(x => x.Text).ToArray();

            _prevSelectedIndex = Base.DefaultOptionIndex;
        }
        
        private SettingsDropdown(SSDropdownSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
            _prevSelectedIndex = Base.DefaultOptionIndex;
        }

        public new SSDropdownSetting Base { get; }
        
        public Action<SettingsDropdown, SettingsDropdownOption, SettingsDropdownOption> OnSelected { get; set; }

        public LockedList<SettingsDropdownOption> Options { get; } = new LockedList<SettingsDropdownOption>();

        public int PreviousIndex => _prevSelectedIndex;
        
        public int DefaultOptionIndex
        {
            get => Base.DefaultOptionIndex;
            set => Base.DefaultOptionIndex = value;
        }

        public int SelectedIndex
        {
            get => Base.SyncSelectionIndexRaw;
            set => Base.SyncSelectionIndexRaw = value;
        }

        public SettingsDropdownOption SelectedOption
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex >= Options.Count)
                    throw new ArgumentOutOfRangeException(nameof(SelectedIndex));

                return Options[SelectedIndex];
            }
        }

        public SettingsDropdown AddOption<T>(T optionValue, string optionText)
            => AddOption(new SettingsDropdownOption<T>(optionValue, optionText));

        public SettingsDropdown AddOption(object optionValue, string optionText)
            => AddOption(new SettingsDropdownOption(optionValue, optionText));

        public SettingsDropdown AddOption(SettingsDropdownOption option)
        {
            if (option is null)
                throw new ArgumentNullException(nameof(option));

            Options.Add(option);
            return this;
        }

        public SettingsDropdownOption GetOption(int optionIndex)
            => TryGetOption(optionIndex, out var option) ? option : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        public SettingsDropdownOption<T> GetOption<T>(int optionIndex)
            => TryGetOption<T>(optionIndex, out var option) ? option : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        public object GetValue(int optionIndex)
            => TryGetValue(optionIndex, out var optionValue) ? optionValue : throw new Exception($"Could not get value of option index {optionIndex}");

        public object GetValueOrDefault(int optionIndex, object defaultValue = null)
            => TryGetValue(optionIndex, out var optionValue) ? optionValue : defaultValue;

        public T GetValue<T>(int optionIndex)
            => TryGetValue<T>(optionIndex, out var optionValue) ? optionValue : throw new Exception($"Could not get value of option index {optionIndex}");

        public T GetValueOrDefault<T>(int optionIndex, T defaultValue = default)
            => TryGetValue<T>(optionIndex, out var optionValue) ? optionValue : defaultValue;

        public string GetText(int optionIndex)
            => TryGetText(optionIndex, out var optionText) ? optionText : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        public bool TryGetOption(int optionIndex, out SettingsDropdownOption dropdownOption)
        {
            if (optionIndex < 0 || optionIndex >= Options.Count)
            {
                dropdownOption = null;
                return false;
            }

            dropdownOption = Options[optionIndex];
            return true;
        }

        public bool TryGetOption<T>(int optionIndex, out SettingsDropdownOption<T> dropdownOption)
        {
            if (optionIndex < 0 || optionIndex >= Options.Count)
            {
                dropdownOption = null;
                return false;
            }

            var option = Options[optionIndex];

            if (option is not SettingsDropdownOption<T> genericOption)
            {
                dropdownOption = null;
                return false;
            }

            dropdownOption = genericOption;
            return true;
        }

        public bool TryGetValue(int optionIndex, out object optionValue)
        {
            if (!TryGetOption(optionIndex, out var option))
            {
                optionValue = null;
                return false;
            }

            optionValue = option.Data;
            return true;
        }

        public bool TryGetValue<T>(int optionIndex, out T optionValue)
        {
            if (!TryGetOption(optionIndex, out var option) || option.Data is null || option.Data is not T castData)
            {
                optionValue = default;
                return false;
            }

            optionValue = castData;
            return true;
        }

        public bool TryGetText(int optionIndex, out string optionText)
        {
            if (!TryGetOption(optionIndex, out var option))
            {
                optionText = null;
                return false;
            }

            optionText = option.Text;
            return true;
        }

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();

            if (_prevSelectedIndex == SelectedIndex)
                return;

            var previousOption = TryGetOption(_prevSelectedIndex, out var previous) ? previous : null;
            var currentOption = TryGetOption(SelectedIndex, out var current) ? current : null;

            _prevSelectedIndex = SelectedIndex;
            
            HandleSelection(previousOption, currentOption);

            OnSelected.InvokeSafe(this, previousOption, currentOption);
        }
        
        public virtual void HandleSelection(SettingsDropdownOption previous, SettingsDropdownOption option) { }

        public override string ToString()
            => $"SettingsDropdown (CustomId={CustomId}; AssignedId={AssignedId}; Ply={Player?.UserId ?? "null"}; Selected={SelectedIndex}; Default={DefaultOptionIndex})";
        
        public static SettingsDropdown Create(string customId, string dropdownLabel, int defaultOptionIndex, SSDropdownSetting.DropdownEntryType dropdownEntryType, Action<SettingsDropdown> dropdownBuilder = null, string dropdownHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(dropdownLabel))
                throw new ArgumentNullException(nameof(dropdownLabel));

            var dropdownId = SettingsManager.GetIntegerId(customId);
            var dropdown = new SSDropdownSetting(dropdownId, dropdownLabel, null, defaultOptionIndex, dropdownEntryType, dropdownHint);
            var setting = new SettingsDropdown(dropdown, customId);

            dropdownBuilder.InvokeSafe(setting);
            dropdown.Options = setting.Options.Select(x => x.Text).ToArray();

            return setting;
        }
    }
}