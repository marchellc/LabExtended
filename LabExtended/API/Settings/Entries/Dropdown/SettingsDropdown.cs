using LabExtended.API.Interfaces;

using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Dropdown
{
    /// <summary>
    /// Represents a configurable dropdown settings entry that allows selection from a list of options, supporting both
    /// value and display text for each option.
    /// </summary>
    public class SettingsDropdown : SettingsEntry, IWrapper<SSDropdownSetting>
    {
        private int _prevSelectedIndex;

        /// <summary>
        /// Initializes a new instance of the SettingsDropdown class with the specified identifier, label, default
        /// option, entry type, and optional configuration.
        /// </summary>
        /// <param name="customId">A unique string identifier for the dropdown. Used to associate the dropdown with a specific setting.</param>
        /// <param name="dropdownLabel">The label text displayed for the dropdown control.</param>
        /// <param name="defaultOptionIndex">The zero-based index of the option that is selected by default. Must be within the range of available
        /// options.</param>
        /// <param name="dropdownEntryType">The type of entries that the dropdown will display. Determines how options are presented and selected.</param>
        /// <param name="dropdownBuilder">An optional action that allows additional configuration of the SettingsDropdown instance after
        /// initialization. Can be null.</param>
        /// <param name="dropdownHint">An optional hint or description displayed alongside the dropdown to provide additional context to the user.
        /// Can be null.</param>
        public SettingsDropdown(
            string customId, 
            string dropdownLabel, 
            
            int defaultOptionIndex, 
            SSDropdownSetting.DropdownEntryType dropdownEntryType, 
            Action<SettingsDropdown>? dropdownBuilder = null, 
            string? dropdownHint = null)
        
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
         
            dropdownBuilder?.InvokeSafe(this);
            
            Base.Options = Options.Select(x => x.Text).ToArray();

            _prevSelectedIndex = Base.DefaultOptionIndex;
        }
        
        private SettingsDropdown(SSDropdownSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
            _prevSelectedIndex = Base.DefaultOptionIndex;
        }

        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSDropdownSetting Base { get; }
        
        /// <summary>
        /// Gets or sets the callback that is invoked when a selection is made in the dropdown.
        /// </summary>
        public Action<SettingsDropdown, SettingsDropdownOption, SettingsDropdownOption> OnSelected { get; set; }

        /// <summary>
        /// Gets the list of dropdown options.
        /// </summary>
        public List<SettingsDropdownOption> Options { get; } = new();

        /// <summary>
        /// Gets the previously selected index before the last change.
        /// </summary>
        public int PreviousIndex => _prevSelectedIndex;

        /// <summary>
        /// Gets or sets the index of the default option that is selected when the dropdown is first displayed.
        /// </summary>
        public int DefaultOptionIndex
        {
            get => Base.DefaultOptionIndex;
            set => Base.DefaultOptionIndex = value;
        }

        /// <summary>
        /// Gets or sets the index of the currently selected option.
        /// </summary>
        public int SelectedIndex
        {
            get => Base.SyncSelectionIndexRaw;
            set => Base.SyncSelectionIndexRaw = value;
        }

        /// <summary>
        /// Gets the currently selected option.
        /// </summary>
        public SettingsDropdownOption SelectedOption
        {
            get
            {
                if (SelectedIndex < 0 || SelectedIndex >= Options.Count)
                    throw new ArgumentOutOfRangeException(nameof(SelectedIndex));

                return Options[SelectedIndex];
            }
        }

        /// <summary>
        /// Synchronizes the options in the dropdown with the underlying base setting.
        /// </summary>
        public void SyncOptions()
            => Base.Options = Options.Select(x => x.Text).ToArray();

        /// <summary>
        /// Adds a new option to the dropdown with the specified value and display text.
        /// </summary>
        /// <typeparam name="T">The type of the value associated with the dropdown option.</typeparam>
        /// <param name="optionValue">The value to associate with the new dropdown option.</param>
        /// <param name="optionText">The text to display for the new dropdown option.</param>
        /// <returns>The current <see cref="SettingsDropdown"/> instance with the new option added.</returns>
        public SettingsDropdown AddOption<T>(T optionValue, string optionText)
            => AddOption(new SettingsDropdownOption<T>(optionValue, optionText));

        /// <summary>
        /// Adds a new option to the dropdown with the specified value and display text.
        /// </summary>
        /// <param name="optionValue">The value associated with the new option. This value is used to identify the option when selected. Cannot be
        /// null.</param>
        /// <param name="optionText">The text to display for the new option in the dropdown. Cannot be null or empty.</param>
        /// <returns>The current <see cref="SettingsDropdown"/> instance with the new option added. This enables method chaining.</returns>
        public SettingsDropdown AddOption(object optionValue, string optionText)
            => AddOption(new SettingsDropdownOption(optionValue, optionText));

        /// <summary>
        /// Adds the specified option to the dropdown list.
        /// </summary>
        /// <param name="option">The option to add to the dropdown. Cannot be null.</param>
        /// <returns>The current <see cref="SettingsDropdown"/> instance, enabling method chaining.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="option"/> is null.</exception>
        public SettingsDropdown AddOption(SettingsDropdownOption option)
        {
            if (option is null)
                throw new ArgumentNullException(nameof(option));

            Options.Add(option);
            return this;
        }

        /// <summary>
        /// Retrieves the settings dropdown option at the specified index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option to retrieve. Must be within the valid range of available options.</param>
        /// <returns>The <see cref="SettingsDropdownOption"/> at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="optionIndex"/> is less than zero or greater than or equal to the number of
        /// available options.</exception>
        public SettingsDropdownOption GetOption(int optionIndex)
            => TryGetOption(optionIndex, out var option) 
            ? option
            : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        /// <summary>
        /// Retrieves the option at the specified index and casts it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the option will be cast.</typeparam>
        /// <param name="optionIndex">The zero-based index of the option to retrieve.</param>
        /// <returns>A SettingsDropdownOption{T} representing the option at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when optionIndex is less than zero or greater than or equal to the number of available options.</exception>
        public SettingsDropdownOption<T> GetOption<T>(int optionIndex)
            => TryGetOption<T>(optionIndex, out var option) 
            ? option 
            : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        /// <summary>
        /// Retrieves the value associated with the specified option index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option whose value to retrieve.</param>
        /// <returns>The value associated with the specified option index.</returns>
        /// <exception cref="Exception">Thrown if no value exists for the specified option index.</exception>
        public object GetValue(int optionIndex)
            => TryGetValue(optionIndex, out var optionValue) 
            ? optionValue 
            : throw new Exception($"Could not get value of option index {optionIndex}");

        /// <summary>
        /// Retrieves the value associated with the specified option index, or returns a default value if the option is
        /// not set.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option whose value to retrieve.</param>
        /// <param name="defaultValue">The value to return if the option at the specified index is not set. This parameter is optional and defaults
        /// to null.</param>
        /// <returns>The value associated with the specified option index if it exists; otherwise, the value of <paramref
        /// name="defaultValue"/>.</returns>
        public object? GetValueOrDefault(int optionIndex, object? defaultValue = null)
            => TryGetValue(optionIndex, out var optionValue) ? optionValue : defaultValue;

        /// <summary>
        /// Retrieves the value of the specified option and returns it as the requested type.
        /// </summary>
        /// <typeparam name="T">The type to which the option value will be cast and returned.</typeparam>
        /// <param name="optionIndex">The zero-based index of the option whose value to retrieve.</param>
        /// <returns>The value of the option at the specified index, cast to type T.</returns>
        /// <exception cref="Exception">Thrown if the value for the specified option index cannot be retrieved or cast to type T.</exception>
        public T GetValue<T>(int optionIndex)
            => TryGetValue<T>(optionIndex, out var optionValue) 
            ? optionValue 
            : throw new Exception($"Could not get value of option index {optionIndex}");

        /// <summary>
        /// Retrieves the value of the option at the specified index, or returns a default value if the option is not
        /// set.
        /// </summary>
        /// <typeparam name="T">The type of the option value to retrieve.</typeparam>
        /// <param name="optionIndex">The zero-based index of the option to retrieve.</param>
        /// <param name="defaultValue">The value to return if the option at the specified index is not set. The default is the default value of
        /// type T.</param>
        /// <returns>The value of the option at the specified index if it is set; otherwise, the specified default value.</returns>
        public T? GetValueOrDefault<T>(int optionIndex, T? defaultValue = default)
            => TryGetValue<T>(optionIndex, out var optionValue) 
            ? optionValue 
            : defaultValue;

        /// <summary>
        /// Retrieves the text associated with the specified option index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option whose text to retrieve. Must be within the valid range of available
        /// options.</param>
        /// <returns>The text corresponding to the specified option index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if optionIndex is less than zero or greater than or equal to the number of available options.</exception>
        public string GetText(int optionIndex)
            => TryGetText(optionIndex, out var optionText) ? optionText : throw new ArgumentOutOfRangeException(nameof(optionIndex));

        /// <summary>
        /// Attempts to retrieve the option at the specified index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option to retrieve. Must be greater than or equal to 0 and less than the total
        /// number of options.</param>
        /// <param name="dropdownOption">When this method returns, contains the option at the specified index if the index is valid; otherwise, null.
        /// This parameter is passed uninitialized.</param>
        /// <returns>true if the option at the specified index was found and assigned to dropdownOption; otherwise, false.</returns>
        public bool TryGetOption(int optionIndex, out SettingsDropdownOption dropdownOption)
        {
            if (optionIndex < 0 || optionIndex >= Options.Count)
            {
                dropdownOption = null!;
                return false;
            }

            dropdownOption = Options[optionIndex];
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the dropdown option at the specified index and cast it to the specified type.
        /// </summary>
        /// <remarks>Use this method to safely attempt to retrieve and cast a dropdown option without
        /// throwing an exception if the cast fails or the index is out of range.</remarks>
        /// <typeparam name="T">The type to which the dropdown option is cast if found.</typeparam>
        /// <param name="optionIndex">The zero-based index of the option to retrieve. Must be greater than or equal to 0 and less than the total
        /// number of options.</param>
        /// <param name="dropdownOption">When this method returns, contains the dropdown option at the specified index cast to type <typeparamref
        /// name="T"/>, if the cast is successful; otherwise, <see langword="null"/>. This parameter is passed
        /// uninitialized.</param>
        /// <returns><see langword="true"/> if the option exists at the specified index and can be cast to type <typeparamref
        /// name="T"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGetOption<T>(int optionIndex, out SettingsDropdownOption<T> dropdownOption)
        {
            if (optionIndex < 0 || optionIndex >= Options.Count)
            {
                dropdownOption = null!;
                return false;
            }

            var option = Options[optionIndex];

            if (option is not SettingsDropdownOption<T> genericOption)
            {
                dropdownOption = null!;
                return false;
            }

            dropdownOption = genericOption;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the value of the option at the specified index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option to retrieve. Must be within the valid range of available options.</param>
        /// <param name="optionValue">When this method returns, contains the value of the option at the specified index if found; otherwise, <see
        /// langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the option value was found and assigned to <paramref name="optionValue"/>;
        /// otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue(int optionIndex, out object optionValue)
        {
            if (!TryGetOption(optionIndex, out var option))
            {
                optionValue = null!;
                return false;
            }

            optionValue = option.Data;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the value of the specified option and cast it to the requested type.
        /// </summary>
        /// <typeparam name="T">The type to which the option value should be cast if found.</typeparam>
        /// <param name="optionIndex">The zero-based index of the option to retrieve.</param>
        /// <param name="optionValue">When this method returns, contains the value of the option cast to type <typeparamref name="T"/> if the
        /// option exists and is of the correct type; otherwise, the default value for type <typeparamref name="T"/>.</param>
        /// <returns><see langword="true"/> if the option exists at the specified index and can be cast to type <typeparamref
        /// name="T"/>; otherwise, <see langword="false"/>.</returns>
        public bool TryGetValue<T>(int optionIndex, out T optionValue)
        {
            if (!TryGetOption(optionIndex, out var option) || option.Data is null || option.Data is not T castData)
            {
                optionValue = default!;
                return false;
            }

            optionValue = castData;
            return true;
        }

        /// <summary>
        /// Attempts to retrieve the display text for the option at the specified index.
        /// </summary>
        /// <param name="optionIndex">The zero-based index of the option whose text to retrieve. Must be within the valid range of available
        /// options.</param>
        /// <param name="optionText">When this method returns, contains the display text of the option at the specified index, if found;
        /// otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the text for the specified option index was found and returned in optionText; otherwise, false.</returns>
        public bool TryGetText(int optionIndex, out string optionText)
        {
            if (!TryGetOption(optionIndex, out var option))
            {
                optionText = null!;
                return false;
            }

            optionText = option.Text;
            return true;
        }

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

            if (_prevSelectedIndex == SelectedIndex)
                return;

            var previousOption = TryGetOption(_prevSelectedIndex, out var previous) ? previous : null;
            var currentOption = TryGetOption(SelectedIndex, out var current) ? current : null;

            _prevSelectedIndex = SelectedIndex;
            
            HandleSelection(previousOption!, currentOption!);

            OnSelected?.InvokeSafe(this, previousOption, currentOption);
        }

        /// <summary>
        /// An overridable method called when a selection is made in the dropdown.
        /// </summary>
        /// <param name="previous">The previously selected option.</param>
        /// <param name="option">The newly selected option.</param>
        public virtual void HandleSelection(SettingsDropdownOption previous, SettingsDropdownOption option) { }

        /// <summary>
        /// Returns a string that represents the current state of the SettingsDropdown instance.
        /// </summary>
        /// <returns>A string containing the values of CustomId, AssignedId, Player.UserId, SelectedIndex, and DefaultOptionIndex
        /// for this SettingsDropdown. If Player is null, "null" is shown for the user ID.</returns>
        public override string ToString()
            => $"SettingsDropdown (CustomId={CustomId}; AssignedId={AssignedId}; Ply={Player?.UserId ?? "null"}; Selected={SelectedIndex}; Default={DefaultOptionIndex})";
        
        /// <summary>
        /// Creates a new settings dropdown with the specified label, options, and configuration.
        /// </summary>
        /// <remarks>Use the <paramref name="dropdownBuilder"/> parameter to add or modify options before
        /// the dropdown is finalized. The <paramref name="defaultOptionIndex"/> should correspond to a valid option
        /// index after options are configured.</remarks>
        /// <param name="customId">A unique identifier for the dropdown. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="dropdownLabel">The display label for the dropdown. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="defaultOptionIndex">The zero-based index of the option that is selected by default.</param>
        /// <param name="dropdownEntryType">The type of entries that the dropdown will contain.</param>
        /// <param name="dropdownBuilder">An optional delegate that can be used to configure additional options or settings for the dropdown before it
        /// is finalized.</param>
        /// <param name="dropdownHint">An optional hint or description to display alongside the dropdown.</param>
        /// <returns>A new instance of <see cref="SettingsDropdown"/> configured with the specified parameters and options.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="customId"/> or <paramref name="dropdownLabel"/> is null, empty, or consists only
        /// of white-space characters.</exception>
        public static SettingsDropdown Create(string customId, string dropdownLabel, int defaultOptionIndex,
            SSDropdownSetting.DropdownEntryType dropdownEntryType, Action<SettingsDropdown>? dropdownBuilder = null, string? dropdownHint = null)
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