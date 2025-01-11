using LabExtended.API.Interfaces;
using LabExtended.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    public class SettingsButton : SettingsEntry, IWrapper<SSButton>
    {
        public SettingsButton(
            string customId, 
            string buttonLabel, 
            string buttonText, 
            string buttonHint = null, 
            float? requiredHeldTimeSeconds = null)
            
            : base(new SSButton(
                SettingsManager.GetIntegerId(customId), 
                
                buttonLabel, 
                buttonText,
                requiredHeldTimeSeconds, 
                buttonHint), 
                
                customId)
        {
            Base = (SSButton)base.Base;
        }

        private SettingsButton(SSButton baseValue, string customId) : base(baseValue, customId)
            => Base = baseValue;
        
        public Action<SettingsButton> OnTriggered { get; set; }

        public new SSButton Base { get; }

        public float? LastTriggerTime { get; private set; }

        public TimeSpan? TimeSinceLastTrigger
        {
            get
            {
                if (!LastTriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastTriggerTime.Value);
            }
        }
        
        public float RequiredHeldTimeSeconds
        {
            get => Base.HoldTimeSeconds;
            set => Base.HoldTimeSeconds = value;
        }

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();

            LastTriggerTime = Time.realtimeSinceStartup;

            HandleTrigger();
            
            OnTriggered.InvokeSafe(this);
        }
        
        public virtual void HandleTrigger() { }

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