using LabExtended.API.Enums;

namespace LabExtended.Utilities.Update;

/// <summary>
/// Tells the <see cref="PlayerUpdateHelper.RegisterUpdates"/> method that a method markes with this attribute
/// should be registered.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PlayerUpdateAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the method's target delay between each call (in seconds).
    /// </summary>
    public float? TimeDelay { get; set; } = null;

    /// <summary>
    /// Gets or sets the round states at which this method will NOT be called.
    /// </summary>
    public RoundState? BlacklistedRoundStates { get; set; } = null;
    
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public PlayerUpdateAttribute() { }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="timeDelay">The delay between each method call (in seconds).</param>
    /// <param name="blacklistedRoundStates">Round states at which this method will NOT be called.</param>
    public PlayerUpdateAttribute(float? timeDelay = null, RoundState? blacklistedRoundStates = null)
    { 
        TimeDelay = timeDelay;
        BlacklistedRoundStates = blacklistedRoundStates;
    }
}