using LabExtended.API.Interfaces;

using UserSettings.ServerSpecific;

namespace LabExtended.API.Settings.Entries;

public class SettingsGroup : SettingsEntry, IWrapper<SSGroupHeader>
{
    public SettingsGroup(string header, bool reducedPadding = false, string headerHint = null) : base(new SSGroupHeader(header, reducedPadding, headerHint), header)
    {
        if (string.IsNullOrWhiteSpace(header))
            throw new ArgumentNullException(nameof(header));

        Base = (SSGroupHeader)base.Base;
    }
    
    public new SSGroupHeader Base { get; }

    public override string ToString()
        => $"SettingsGroup (Header={Base?.Label ?? "null"}; Ply={Player?.UserId ?? "null"})";
}