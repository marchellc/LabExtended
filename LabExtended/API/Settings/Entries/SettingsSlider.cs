using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsSlider : SettingsEntry, IWrapper<SSSliderSetting>
    {
        public SettingsSlider(SSSliderSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSSliderSetting Base { get; }
        
        public Action<SettingsSlider> OnMoved { get; set; }

        public float Value => Base.SyncFloatValue;

        public bool ShouldSyncDrag
        {
            get => Base.SyncDragging;
            set => Base.SyncDragging = value;
        }

        public bool IsInteger
        {
            get => Base.Integer;
            set => Base.Integer = value;
        }

        public float DefaultValue
        {
            get => Base.DefaultValue;
            set => Base.DefaultValue = value;
        }

        public float MinValue
        {
            get => Base.MinValue;
            set => Base.MinValue = value;
        }

        public float MaxValue
        {
            get => Base.MaxValue;
            set => Base.MaxValue = value;
        }

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();
            OnMoved.InvokeSafe(this);
        }

        public static SettingsSlider Create(string customId, string sliderLabel, float minValue, float maxValue, float defaultValue = 0f, bool isInteger = false, string valueToStringFormat = "0.##", string finalDisplayFormat = "{0}", string sliderHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(sliderLabel))
                throw new ArgumentNullException(nameof(sliderLabel));

            var sliderId = SettingsManager.GetIntegerId(customId);
            var slider = new SSSliderSetting(sliderId, sliderLabel, minValue, maxValue, defaultValue, isInteger, valueToStringFormat, finalDisplayFormat, sliderHint);

            return new SettingsSlider(slider, customId);
        }
    }
}