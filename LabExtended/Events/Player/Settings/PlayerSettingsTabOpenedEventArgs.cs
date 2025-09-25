using LabExtended.API;

namespace LabExtended.Events.Player.Settings;

/// <summary>
/// Gets called when a player opens their server settings tab.
/// </summary>
public class PlayerSettingsTabOpenedEventArgs : EventArgs
{
    /// <summary>
    /// The player who opened their settings tab.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Creates a new <see cref="PlayerSettingsTabOpenedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player who opened their settings tab.</param>
    public PlayerSettingsTabOpenedEventArgs(ExPlayer player) 
        => Player = player;
}