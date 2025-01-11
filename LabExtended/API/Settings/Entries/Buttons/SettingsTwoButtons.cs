using LabExtended.API.Interfaces;
using LabExtended.Extensions;
using UnityEngine;
using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    public class SettingsTwoButtons : SettingsEntry, IWrapper<SSTwoButtonsSetting>
    {
        private bool _previousIsSyncB;

        public SettingsTwoButtons(
            string customId, 
            string buttonLabel, 
            
            string buttonAText, 
            string buttonBText,
            
            bool isDefaultButtonB = true, 
            
            string buttonsHint = null)
            : base(new SSTwoButtonsSetting(
                SettingsManager.GetIntegerId(customId), 
                
                buttonLabel, 
                
                buttonAText, 
                buttonBText,
                    
                isDefaultButtonB, 
                buttonsHint), 
                
                customId)
        {
            Base = (SSTwoButtonsSetting)base.Base;
            _previousIsSyncB = Base.DefaultIsB;
        }
        
        private SettingsTwoButtons(SSTwoButtonsSetting baseValue, string customId) : base(baseValue, customId)
        {
            Base = baseValue;
            _previousIsSyncB = Base.DefaultIsB;
        }
        
        public Action<SettingsTwoButtons> OnTriggered { get; set; }

        public new SSTwoButtonsSetting Base { get; }
        
        public float? LastATriggerTime { get; private set; }
        public float? LastBTriggerTime { get; private set; }

        public TimeSpan? TimeSinceLastATrigger
        {
            get
            {
                if (!LastATriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastATriggerTime.Value);
            }
        }
        
        public TimeSpan? TimeSinceLastBTrigger
        {
            get
            {
                if (!LastBTriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastBTriggerTime.Value);
            }
        }

        public string ButtonAText
        {
            get => Base.OptionA;
            set => Base.OptionA = value;
        }

        public string ButtonBText
        {
            get => Base.OptionB;
            set => Base.OptionB = value;
        }

        public bool IsDefaultButtonB
        {
            get => Base.DefaultIsB;
            set => Base.DefaultIsB = value;
        }

        public bool IsAButtonActive => Base.SyncIsA;
        public bool IsBButtonActive => Base.SyncIsB;

        public bool WasAButtonActive => !_previousIsSyncB;
        public bool WasBButtonActive => _previousIsSyncB;

        /// <inheritdoc />
        internal override void InternalOnUpdated()
        {
            base.InternalOnUpdated();

            if (_previousIsSyncB != IsBButtonActive)
            {
                _previousIsSyncB = IsBButtonActive;

                if (IsAButtonActive)
                    LastATriggerTime = Time.realtimeSinceStartup;
                else
                    LastBTriggerTime = Time.realtimeSinceStartup;
                
                HandleTrigger(IsBButtonActive);
                
                OnTriggered.InvokeSafe(this);
            }
        }

        public virtual void HandleTrigger(bool isB) { }
        
        public static SettingsTwoButtons Create(string customId, string buttonLabel, string buttonAText, string buttonBText, bool isDefaultButtonB = true, string buttonsHint = null)
        {
            if (string.IsNullOrWhiteSpace(customId))
                throw new ArgumentNullException(nameof(customId));

            if (string.IsNullOrWhiteSpace(buttonLabel))
                throw new ArgumentNullException(nameof(buttonLabel));

            if (string.IsNullOrWhiteSpace(buttonAText))
                throw new ArgumentNullException(nameof(buttonAText));

            if (string.IsNullOrWhiteSpace(buttonBText))
                throw new ArgumentNullException(nameof(buttonBText));

            var buttonId = SettingsManager.GetIntegerId(customId);
            var button = new SSTwoButtonsSetting(buttonId, buttonLabel, buttonAText, buttonBText, isDefaultButtonB, buttonsHint);

            return new SettingsTwoButtons(button, customId);
        }
    }
}