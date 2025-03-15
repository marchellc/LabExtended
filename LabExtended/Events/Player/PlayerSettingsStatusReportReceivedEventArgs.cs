using LabExtended.API;

using UserSettings.ServerSpecific;

namespace LabExtended.Events.Player;

public class PlayerSettingsStatusReportReceivedEventArgs : EventArgs
{
    /// <summary>
    /// The player whose report was received.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// The received report.
    /// </summary>
    public SSSUserStatusReport NewReport { get; }
    
    /// <summary>
    /// The current report.
    /// </summary>
    public SSSUserStatusReport? CurrentReport { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSettingsStatusReportReceivedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player whose report was received.</param>
    /// <param name="newReport">The received report.</param>
    /// <param name="currentReport">The last received report.</param>
    public PlayerSettingsStatusReportReceivedEventArgs(ExPlayer player, SSSUserStatusReport newReport, SSSUserStatusReport? currentReport)
    {
        Player = player;
        NewReport = newReport;
        CurrentReport = currentReport;
    }
}