using LabExtended.API;

using UserSettings.ServerSpecific;

namespace LabExtended.Events.Settings;

public class SettingsStatusReportReceivedArgs
{
    public ExPlayer Player { get; }
    
    public SSSUserStatusReport NewReport { get; }
    public SSSUserStatusReport? CurrentReport { get; }

    public SettingsStatusReportReceivedArgs(ExPlayer player, SSSUserStatusReport newReport, SSSUserStatusReport? currentReport)
    {
        Player = player;
        NewReport = newReport;
        CurrentReport = currentReport;
    }
}