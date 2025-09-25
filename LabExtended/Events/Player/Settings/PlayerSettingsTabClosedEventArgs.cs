using LabExtended.API;

namespace LabExtended.Events.Player.Settings;

/// <summary>
/// Gets called after a player closes their server settings tab.
/// </summary>
public class PlayerSettingsTabClosedEventArgs : EventArgs
{
    /// <summary>
    /// The player who closed their server settings tab.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Creates a new <see cref="PlayerSettingsTabClosedEventArgs"/>
    /// </summary>
    /// <param name="player">Player who closed their settings tab.</param>
    public PlayerSettingsTabClosedEventArgs(ExPlayer player) 
        => Player = player;
}