namespace LabExtended.API.Hints.Interfaces;

/// <summary>
/// Allows a hint element to modify the controller's internal tick delay.
/// </summary>
public interface IHintRateModifier
{
    /// <summary>
    /// Gets the element's desired delay (in seconds).
    /// </summary>
    /// <param name="targetDelay">The currently targeted tick delay (in seconds).</param>
    /// <returns>The desired tick delay (in seconds).</returns>
    float GetDesiredDelay(float targetDelay);
}