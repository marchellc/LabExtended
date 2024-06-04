namespace LabExtended.Utilities
{
    /// <summary>
    /// A tool to help with version compatibility by providing an easy version range.
    /// </summary>
    public struct VersionRange
    {
        /// <summary>
        /// The minimal supported version.
        /// </summary>
        public readonly Version Minimal;

        /// <summary>
        /// The maximum supported version.
        /// </summary>
        public readonly Version Maximal;

        /// <summary>
        /// The only supported version.
        /// </summary>
        public readonly Version Specific;

        /// <summary>
        /// Creates a new version range.
        /// </summary>
        /// <param name="minimal">The minimal supported version.</param>
        /// <param name="maximal">The maximum supported version.</param>
        public VersionRange(Version minimal, Version maximal)
        {
            Minimal = minimal;
            Maximal = maximal;
        }

        /// <summary>
        /// Creates a single-version range.
        /// </summary>
        /// <param name="specific">The only supported version.</param>
        public VersionRange(Version specific)
            => Specific = specific;

        /// <summary>
        /// Gets a value indicating whether or not the specified version is in range.
        /// </summary>
        /// <param name="version">The version to check compatibility of.</param>
        /// <returns>true if the version is in range, otherwise false.</returns>
        public bool InRange(Version version)
        {
            if (Specific != null && (version.Major != Specific.Major || version.Minor != Specific.Minor || version.Build != Specific.Build))
                return false;

            if (Minimal != null && (version.Major < Minimal.Major || version.Minor < Minimal.Minor || version.Build < Minimal.Build))
                return false;

            if (Maximal != null && (version.Major > Maximal.Major || version.Minor > Maximal.Minor || version.Build > Maximal.Build))
                return false;

            return true;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var str = "(";

            if (Minimal != null)
                str += $"Min={Minimal} ";

            if (Maximal != null)
                str += $"Max={Maximal} ";

            if (Specific != null)
                str += $"{Specific}";

            return (str + ")").Trim();
        }
    }
}