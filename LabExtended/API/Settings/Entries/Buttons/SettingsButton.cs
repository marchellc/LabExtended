using LabExtended.API.Interfaces;
using LabExtended.Extensions;

using UnityEngine;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries.Buttons
{
    /// <summary>
    /// A pressable button.
    /// </summary>
    public class SettingsButton : SettingsEntry, IWrapper<SSButton>
    {
        /// <summary>
        /// Initializes a new instance of the SettingsButton class with the specified button configuration and behavior.
        /// </summary>
        /// <param name="customId">A unique identifier for the button, used to associate the button with a specific setting.</param>
        /// <param name="buttonLabel">The text displayed as the button's label in the user interface.</param>
        /// <param name="buttonText">The text shown on the button itself.</param>
        /// <param name="buttonHint">An optional hint or tooltip text displayed to provide additional information about the button. Can be null
        /// if no hint is required.</param>
        /// <param name="requiredHeldTimeSeconds">The minimum time, in seconds, that the button must be held down to activate. If null, the button does not
        /// require a hold duration.</param>
        public SettingsButton(
            string customId, 
            string buttonLabel, 
            string buttonText, 
            string? buttonHint = null, 
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
        
        /// <summary>
        /// Gets or sets the callback to invoke when the settings button is triggered.
        /// </summary>
        public Action<SettingsButton> OnTriggered { get; set; }

        /// <summary>
        /// Gets the base entry.
        /// </summary>
        public new SSButton Base { get; }

        /// <summary>
        /// Gets the time, in seconds (using <see cref="Time.realtimeSinceStartup"/>), at which the last trigger event occurred, or null if no trigger has occurred.
        /// </summary>
        public float? LastTriggerTime { get; private set; }

        /// <summary>
        /// Gets the time (using <see cref="Time.realtimeSinceStartup"/>), at which the last trigger event occurred, or null if no trigger has occurred.
        /// </summary>
        public TimeSpan? TimeSinceLastTrigger
        {
            get
            {
                if (!LastTriggerTime.HasValue)
                    return null;

                return TimeSpan.FromMilliseconds(Time.realtimeSinceStartup - LastTriggerTime.Value);
            }
        }
        
        /// <summary>
        /// Gets or sets the required duration, in seconds, that the button must be held to trigger successfully.
        /// </summary>
        public float RequiredHeldTimeSeconds
        {
            get => Base.HoldTimeSeconds;
            set => Base.HoldTimeSeconds = value;
        }

        /// <inheritdoc />
        internal override void Internal_Updated()
        {
            base.Internal_Updated();

            LastTriggerTime = Time.realtimeSinceStartup;

            HandleTrigger();
            
            OnTriggered.InvokeSafe(this);
        }
        
        /// <summary>
        /// An overridable method called when the button is pressed.
        /// </summary>
        public virtual void HandleTrigger() { }

        /// <summary>
        /// Returns a string that represents the current settings button, including its custom ID, assigned ID, and
        /// associated player user ID.
        /// </summary>
        /// <returns>A string containing the custom ID, assigned ID, and player user ID of the settings button. If no player is
        /// assigned, the player user ID is represented as "null".</returns>
        public override string ToString()
            => $"SettingsButton (CustomId={CustomId}; AssignedId={AssignedId}; Ply={Player?.UserId ?? "null"})";

        /// <summary>
        /// Creates a new instance of the SettingsButton class with the specified identifier, label, text, and optional
        /// settings.
        /// </summary>
        /// <param name="customId">A unique string identifier for the button. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="buttonLabel">The label displayed on the button. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="buttonText">The text shown on the button. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="buttonHint">An optional hint or tooltip text for the button. May be null if no hint is required.</param>
        /// <param name="requiredHeldTimeSeconds">The optional duration, in seconds, that the button must be held before activation. If null, the button does
        /// not require a hold.</param>
        /// <returns>A new SettingsButton instance configured with the specified parameters.</returns>
        /// <exception cref="ArgumentNullException">Thrown if customId, buttonLabel, or buttonText is null, empty, or consists only of white-space characters.</exception>
        public static SettingsButton Create(string customId, string buttonLabel, string buttonText, string? buttonHint = null, float? requiredHeldTimeSeconds = null)
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