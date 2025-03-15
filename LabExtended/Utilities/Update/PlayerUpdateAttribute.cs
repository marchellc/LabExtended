using LabExtended.API.Enums;

namespace LabExtended.Utilities.Update;

/// <summary>
/// Tells the <see cref="PlayerUpdateHelper.RegisterUpdates"/> method that a method marked with this attribute
/// should be registered.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PlayerUpdateAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the method's target delay between each call (in seconds).
    /// </summary>
    public float TimeDelay { get; set; } = -1f;

    /// <summary>
    /// Gets or sets the only round states at which this method will be called.
    /// </summary>
    public RoundState WhitelistedRoundStates { get; set; } = RoundState.Unknown;
    
    /// <summary>
    /// Gets or sets the round states at which this method will NOT be called.
    /// </summary>
    public RoundState BlacklistedRoundStates { get; set; } = RoundState.Unknown;
    
    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public PlayerUpdateAttribute() { }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="timeDelay">The delay between each method call (in seconds), values below 0 disable this delay.</param>
    /// <param name="whitelistedRoundStates">Round states at which this method will be executed.</param>
    /// <param name="blacklistedRoundStates">Round states at which this method will NOT be called.</param>
    public PlayerUpdateAttribute(float timeDelay = -1f, 
        RoundState blacklistedRoundStates = RoundState.Unknown,
        RoundState whitelistedRoundStates = RoundState.Unknown)
    { 
        TimeDelay = timeDelay;
        WhitelistedRoundStates = whitelistedRoundStates;
        BlacklistedRoundStates = blacklistedRoundStates;
    }
}