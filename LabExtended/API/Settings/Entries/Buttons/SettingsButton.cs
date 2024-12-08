using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    public class SettingsButton : SettingsEntry, IWrapper<SSButton>
    {
        public SettingsButton(SSButton baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
        }

        public new SSButton Base { get; }
        
        public Action<SettingsButton> OnTriggered { get; set; }

        public float RequiredHeldTimeSeconds
        {
            get => Base.HoldTimeSeconds;
            set => Base.HoldTimeSeconds = value;
        }

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();
            OnTriggered.InvokeSafe(this);
        }

        public static SettingsButton Create(string customId, string buttonLabel, string buttonText, string buttonHint = null, float? requiredHeldTimeSeconds = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(buttonLabel))
                throw new ArgumentNullException(nameof(buttonLabel));

            if (string.IsNullOrWhiteSpace(buttonText))
                throw new ArgumentNullException(nameof(buttonText));

            var buttonId = SettingsManager.GetIntegerId(customId);
            var button = new SSButton(buttonId, buttonLabel, buttonText, requiredHeldTimeSeconds, buttonHint);

            return new SettingsButton(button, customId);
        }
    }
}