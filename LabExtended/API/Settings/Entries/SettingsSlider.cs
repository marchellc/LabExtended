using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    /// <summary>
    /// Represents a configurable slider setting entry with support for floating-point or integer values, value
    /// formatting, and change notification callbacks.
    /// </summary>
    public class SettingsSlider : SettingsEntry, IWrapper<SSSliderSetting>
    {
        private float _prevValue;

        /// <summary>
        /// Initializes a new instance of the SettingsSlider class with the specified slider configuration and display
        /// options.
        /// </summary>
        /// <remarks>Use this constructor to create a slider setting with custom value ranges, formatting,
        /// and display options. The slider can be configured to accept only integer values or floating-point values,
        /// and its appearance can be customized using the provided format strings.</remarks>
        /// <param name="customId">A unique identifier for the slider setting. Used to distinguish this slider from others in the settings
        /// system. Cannot be null or empty.</param>
        /// <param name="sliderLabel">The text label displayed alongside the slider to describe its purpose to the user.</param>
        /// <param name="minValue">The minimum value that the slider can represent.</param>
        /// <param name="maxValue">The maximum value that the slider can represent.</param>
        /// <param name="defaultValue">The value the slider is set to by default when first initialized. Must be within the range defined by
        /// minValue and maxValue.</param>
        /// <param name="isInteger">true to restrict the slider to integer values only; otherwise, false to allow floating-point values.</param>
        /// <param name="valueToStringFormat">A numeric format string used to convert the slider's value to a string for display. For example, "0.##".</param>
        /// <param name="finalDisplayFormat">A composite format string used to display the final value, where "{0}" will be replaced with the formatted
        /// slider value.</param>
        /// <param name="sliderHint">An optional hint or tooltip text displayed to provide additional information about the slider's function.
        /// Can be null.</param>
        public SettingsSlider(
            string customId,
            string sliderLabel,

            float minValue,
            float maxValue,
            float defaultValue = 0f,

            bool isInteger = false,

            string valueToStringFormat = "0.##",
            string finalDisplayFormat = "{0}",
            string? sliderHint = null) : base(new SSSliderSetting(
                SettingsManager.GetIntegerId(customId),
                
                sliderLabel,
                minValue,
                maxValue,
                defaultValue,
                isInteger,
                valueToStringFormat,
                finalDisplayFormat,
                sliderHint), 
            
                customId)
        {
            Base = (SSSliderSetting)base.Base;

            _prevValue = Base.DefaultValue;
        }
        
        private SettingsSlider(SSSliderSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
            _prevValue = baseValue.DefaultValue;
        }
        
        /// <summary>
        /// Gets or sets the callback that is invoked when the slider is moved.
        /// </summary>
        public Action<SettingsSlider> OnMoved { get; set; }

        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSSliderSetting Base { get; }

        /// <summary>
        /// Gets the current value of the slider.
        /// </summary>
        public float Value => Base.SyncFloatValue;

        /// <summary>
        /// Gets the previous value of the slider before the last change.
        /// </summary>
        public float PreviousValue => _prevValue;
        
        /// <summary>
        /// Gets the fill level as a percentage of the maximum value.
        /// </summary>
        public int FillPercentage => (int)Math.Round((double)(100 * Value) / MaxValue);

        /// <summary>
        /// Gets the percentage of the slider that is empty.
        /// </summary>
        public int EmptyPercentage => 100 - FillPercentage;

        /// <summary>
        /// Whether or not slider dragging should be synced to the server.
        /// </summary>
        public bool ShouldSyncDrag
        {
            get => Base.SyncDragging;
            set => Base.SyncDragging = value;
        }

        /// <summary>
        /// Whether or not the value is an integer.
        /// </summary>
        public bool IsInteger
        {
            get => Base.Integer;
            set => Base.Integer = value;
        }

        /// <summary>
        /// Gets or sets the default value of the slider.
        /// </summary>
        public float DefaultValue
        {
            get => Base.DefaultValue;
            set => Base.DefaultValue = value;
        }

        /// <summary>
        /// Gets or sets the minimum value of the slider.
        /// </summary>
        public float MinValue
        {
            get => Base.MinValue;
            set => Base.MinValue = value;
        }

        /// <summary>
        /// Gets or sets the maximum value of the slider.
        /// </summary>
        public float MaxValue
        {
            get => Base.MaxValue;
            set => Base.MaxValue = value;
        }

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

            if (_prevValue == Value)
                return;

            HandleMove(_prevValue, Value);
            
            OnMoved.InvokeSafe(this);
            
            _prevValue = Value;
        }
        
        /// <summary>
        /// An overridable method called when the slider's value changes.
        /// </summary>
        /// <param name="previousValue">The previous value of the slider.</param>
        /// <param name="newValue">The new value of the slider.</param>
        public virtual void HandleMove(float previousValue, float newValue) { }
        
        /// <summary>
        /// Returns a string that represents the current state of the SettingsSlider, including its identifiers,
        /// percentage, value, range, and associated player.
        /// </summary>
        /// <returns>A string containing the CustomId, AssignedId, fill percentage, value, maximum and minimum values, and the
        /// user ID of the associated player if available; otherwise, "null" for the player.</returns>
        public override string ToString()
            => $"SettingsSlider (CustomId={CustomId}; AssignedId={AssignedId}; Percentage={FillPercentage}%; Value={Value}; Max={MaxValue}; Min={MinValue}; Ply={Player?.UserId ?? "null"})";

        /// <summary>
        /// Creates a new settings slider with the specified configuration parameters.
        /// </summary>
        /// <param name="customId">A unique identifier for the slider. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="sliderLabel">The display label for the slider. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="minValue">The minimum value that the slider can represent.</param>
        /// <param name="maxValue">The maximum value that the slider can represent.</param>
        /// <param name="defaultValue">The initial value of the slider when it is first created. Defaults to 0.</param>
        /// <param name="isInteger">true to restrict the slider to integer values; otherwise, false.</param>
        /// <param name="valueToStringFormat">A numeric format string used to convert the slider's value to a string for display. Defaults to "0.##".</param>
        /// <param name="finalDisplayFormat">A composite format string used to display the formatted value. Defaults to "{0}".</param>
        /// <param name="sliderHint">An optional hint or description to display alongside the slider. Can be null.</param>
        /// <returns>A new instance of the SettingsSlider class configured with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId or sliderLabel is null, empty, or consists only of white-space characters.</exception>
        public static SettingsSlider Create(string customId, string sliderLabel, float minValue, float maxValue, float defaultValue = 0f,
            bool isInteger = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string? sliderHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(sliderLabel))
                throw new ArgumentNullException(nameof(sliderLabel));

            var sliderId = SettingsManager.GetIntegerId(customId);
            var slider = new SSSliderSetting(sliderId, sliderLabel, minValue, maxValue, defaultValue, isInteger, valueToStringFormat, 
                finalDisplayFormat, sliderHint);

            return new SettingsSlider(slider, customId);
        }
    }
}