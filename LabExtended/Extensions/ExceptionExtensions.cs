namespace LabExtended.Extensions
{
    /// <summary>
    /// A class that holds extensions for the <see cref="Exception"/> class.
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns a NW API log color formatted string of the exception's <see cref="Exception.ToString"/> method.
        /// </summary>
        /// <param name="ex">The exception to get a colored string of.</param>
        /// <returns>A NW API log color formatted string of the exception's <see cref="Exception.ToString"/> method.</returns>
        public static string ToColoredString(this Exception ex)
            => ex.ToString()
                .Replace("at", "&3at&r")
                .Replace("(", "&2(&r")
                .Replace(")", "&2)&r")
                .Replace(".", "&1.&r");
    }
}