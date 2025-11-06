using System.ComponentModel;

using YamlDotNet.Serialization;

namespace LabExtended.Utilities
{
    /// <summary>
    /// Represents a floating-point range with configurable minimum and maximum values, and provides methods for
    /// range-based operations.
    /// </summary>
    public class RangeFloat
    {
        /// <summary>
        /// Gets or sets the maximum allowable value.
        /// </summary>
        [Description("Sets the maximum value of the range.")]
        public float MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the minimum allowable value.
        /// </summary>
        [Description("Sets the minimum value of the range.")]
        public float MinValue { get; set; }

        /// <summary>
        /// Gets a random floating-point value within the configured minimum and maximum range.
        /// </summary>
        /// <remarks>The returned value is generated using Unity's random number generator and will be
        /// greater than or equal to the minimum value and less than the maximum value. The range is determined by the
        /// values of the MinValue and MaxValue properties.</remarks>
        [YamlIgnore]
        public float Random => UnityEngine.Random.Range(MinValue, MaxValue);

        /// <summary>
        /// Determines whether the specified value falls within the inclusive range defined by MinValue and MaxValue.
        /// </summary>
        /// <param name="value">The value to test for inclusion within the range. Must be a finite floating-point number.</param>
        /// <returns>true if value is greater than or equal to MinValue and less than or equal to MaxValue; otherwise, false.</returns>
        public bool InRange(float value)
            => value >= MinValue && value <= MaxValue;
    }
}