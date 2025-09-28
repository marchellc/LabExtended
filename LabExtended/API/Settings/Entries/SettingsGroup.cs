using LabExtended.API.Interfaces;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries;

/// <summary>
/// Represents a settings group header.
/// </summary>
public class SettingsGroup : SettingsEntry, IWrapper<SSGroupHeader>
{
    /// <summary>
    /// Initializes a new instance of the SettingsGroup class with the specified header and optional settings.
    /// </summary>
    /// <param name="header">The text to display as the group header. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <param name="reducedPadding">true to use reduced padding for the group header; otherwise, false. The default is false.</param>
    /// <param name="headerHint">An optional hint or tooltip to display for the group header. Can be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if header is null, empty, or consists only of white-space characters.</exception>
    public SettingsGroup(string header, bool reducedPadding = false, string? headerHint = null) 
        : base(new SSGroupHeader(header, reducedPadding, headerHint), header)
    {
        if (string.IsNullOrWhiteSpace(header))
            throw new ArgumentNullException(nameof(header));

        Base = (SSGroupHeader)base.Base;
    }
    
    /// <summary>
    /// Gets the base entry.
    /// </summary>
    public new SSGroupHeader Base { get; }

    /// <summary>
    /// Returns a string that represents the current settings group, including the header label and player user ID.
    /// </summary>
    /// <returns>A string containing the header label and player user ID for the settings group. If either value is not set,
    /// "null" is used in its place.</returns>
    public override string ToString()
        => $"SettingsGroup (Header={Base?.Label ?? "null"}; Ply={Player?.UserId ?? "null"})";
}