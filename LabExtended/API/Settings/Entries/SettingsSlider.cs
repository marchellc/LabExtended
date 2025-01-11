using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries
{
    public class SettingsSlider : SettingsEntry, IWrapper<SSSliderSetting>
    {
        private float _prevValue;

        public SettingsSlider(
            string customId,
            string sliderLabel,

            float minValue,
            float maxValue,
            float defaultValue = 0f,

            bool isInteger = false,

            string valueToStringFormat = "0.##",
            string finalDisplayFormat = "{0}",
            string sliderHint = null) : base(new SSSliderSetting(
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
        
        public Action<SettingsSlider> OnMoved { get; set; }

        public new SSSliderSetting Base { get; }

        public float Value => Base.SyncFloatValue;
        public float PreviousValue => _prevValue;
        
        public int FillPercentage => (int)Math.Round((double)(100 * Value) / MaxValue);
        public int EmptyPercentage => 100 - FillPercentage;

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

            if (_prevValue == Value)
                return;

            HandleMove(_prevValue, Value);
            
            OnMoved.InvokeSafe(this);
            
            _prevValue = Value;
        }
        
        public virtual void HandleMove(float previousValue, float newValue) { }
        
        public override string ToString()
            => $"SettingsSlider (CustomId={CustomId}; AssignedId={AssignedId}; Percentage={FillPercentage}%; Value={Value}; Max={MaxValue}; Min={MinValue}; Ply={Player?.UserId ?? "null"})";

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