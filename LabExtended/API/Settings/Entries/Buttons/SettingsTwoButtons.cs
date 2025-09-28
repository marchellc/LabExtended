using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UnityEngine;
using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    /// <summary>
    /// An entry with two buttons (A and B) used as a selection.
    /// </summary>
    public class SettingsTwoButtons : SettingsEntry, IWrapper<SSTwoButtonsSetting>
    {
        private bool _previousIsSyncB;

        /// <summary>
        /// Initializes a new instance of the SettingsTwoButtons class with the specified button labels, texts, and
        /// configuration options.
        /// </summary>
        /// <param name="customId">A unique identifier for the setting. This value is used to distinguish this setting from others and must not
        /// be null or empty.</param>
        /// <param name="buttonLabel">The label displayed above the two buttons. This provides context or a description for the button group.</param>
        /// <param name="buttonAText">The text displayed on the first button (Button A).</param>
        /// <param name="buttonBText">The text displayed on the second button (Button B).</param>
        /// <param name="isDefaultButtonB">true to make Button B the default selection; otherwise, false to make Button A the default.</param>
        /// <param name="buttonsHint">An optional hint or description displayed alongside the buttons to assist the user. Can be null if no hint
        /// is needed.</param>
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
        
        /// <summary>
        /// Gets or sets the callback that is invoked when the trigger event occurs.
        /// </summary>
        public Action<SettingsTwoButtons> OnTriggered { get; set; }

        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSTwoButtonsSetting Base { get; }
        
        /// <summary>
        /// Gets the last trigger time (in seconds) of button A (using <see cref="Time.realtimeSinceStartup"/>).
        /// </summary>
        public float? LastATriggerTime { get; private set; }

        /// <summary>
        /// Gets the last trigger time (in seconds) of button B (using <see cref="Time.realtimeSinceStartup"/>).
        /// </summary>
        public float? LastBTriggerTime { get; private set; }

        /// <summary>
        /// Gets the last trigger time of button A (using <see cref="Time.realtimeSinceStartup"/>).
        /// </summary>
        public TimeSpan? TimeSinceLastATrigger
        {
            get
            {
                if (!LastATriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastATriggerTime.Value);
            }
        }

        /// <summary>
        /// Gets the last trigger time of button B (using <see cref="Time.realtimeSinceStartup"/>).
        /// </summary>
        public TimeSpan? TimeSinceLastBTrigger
        {
            get
            {
                if (!LastBTriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastBTriggerTime.Value);
            }
        }

        /// <summary>
        /// Gets or sets the label of the A button.
        /// </summary>
        public string ButtonAText
        {
            get => Base.OptionA;
            set => Base.OptionA = value;
        }

        /// <summary>
        /// Gets or sets the label of the B button.
        /// </summary>
        public string ButtonBText
        {
            get => Base.OptionB;
            set => Base.OptionB = value;
        }

        /// <summary>
        /// Whether or not the B button shouldd be selected as default.
        /// </summary>
        public bool IsDefaultButtonB
        {
            get => Base.DefaultIsB;
            set => Base.DefaultIsB = value;
        }

        /// <summary>
        /// Whether or not the A button is selected.
        /// </summary>
        public bool IsAButtonActive => Base.SyncIsA;

        /// <summary>
        /// Whether or not the B button is selected.
        /// </summary>
        public bool IsBButtonActive => Base.SyncIsB;

        /// <summary>
        /// Whether or not the A button was selected before the last update.
        /// </summary>
        public bool WasAButtonActive => !_previousIsSyncB;

        /// <summary>
        /// Whether or not the B button was selected before the last update.
        /// </summary>
        public bool WasBButtonActive => _previousIsSyncB;

        /// <summary>
        /// Returns one of two provided options based on whether a button is currently active.
        /// </summary>
        /// <typeparam name="T">The type of the options to select from.</typeparam>
        /// <param name="primaryOption">The option to return if button A is active.</param>
        /// <param name="secondaryOption">The option to return if button B is active.</param>
        /// <returns>The value of <paramref name="primaryOption"/> if a button is active; otherwise, the value of <paramref
        /// name="secondaryOption"/>.</returns>
        public T GetOption<T>(T primaryOption, T secondaryOption)
            => IsAButtonActive ? primaryOption : secondaryOption;

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

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

        /// <summary>
        /// An overridable method called when the selected button is changed.
        /// </summary>
        /// <param name="isB">Whether or not the selected button is the B button.</param>
        public virtual void HandleTrigger(bool isB) { }

        /// <summary>
        /// Returns a string that represents the current state of the SettingsTwoButtons instance.
        /// </summary>
        /// <returns>A string containing the values of CustomId, AssignedId, Player.UserId (or "null" if Player is null), and
        /// IsAButtonActive.</returns>
        public override string ToString()
            => $"SettingsTwoButtons (CustomId={CustomId}; AssignedId={AssignedId}; Ply={Player?.UserId ?? "null"}; IsA={IsAButtonActive})";

        /// <summary>
        /// Creates a new instance of the SettingsTwoButtons class with the specified button configuration.
        /// </summary>
        /// <param name="customId">A unique identifier for the button setting. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <param name="buttonLabel">The label displayed above the two buttons. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="buttonAText">The text displayed on the first button. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="buttonBText">The text displayed on the second button. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="isDefaultButtonB">true to make the second button the default selection; otherwise, false.</param>
        /// <param name="buttonsHint">An optional hint or description displayed below the buttons. Can be null.</param>
        /// <returns>A new SettingsTwoButtons instance configured with the specified labels, button texts, and options.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId, buttonLabel, buttonAText, or buttonBText is null, empty, or consists only of white-space
        /// characters.</exception>
        public static SettingsTwoButtons Create(string customId, string buttonLabel, string buttonAText, string buttonBText, 
            bool isDefaultButtonB = true, string? buttonsHint = null)
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