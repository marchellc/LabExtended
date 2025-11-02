using System.ComponentModel;

namespace LabExtended.Core.Configs.Sections
{
    /// <summary>
    /// Represents configuration settings for hint display behavior, including update intervals and display duration.
    /// </summary>
    public class HintSection
    {
        /// <summary>
        /// Gets or sets the delay, in milliseconds, between each hint update.
        /// </summary>
        [Description("The delay between each hint update (in milliseconds).")]
        public int UpdateInterval { get; set; } = 500;

        /// <summary>
        /// Gets or sets the duration, in seconds, for which the hint is displayed.
        /// </summary>
        [Description("Duration of the displayed hint.")]
        public float HintDuration { get; set; } = 2.5f;
    }
}