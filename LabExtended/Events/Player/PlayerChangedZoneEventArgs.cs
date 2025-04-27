using LabExtended.API;

using MapGeneration;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a player changes the zone they are currently in.
/// </summary>
public class PlayerChangedZoneEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player who changed zones.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the new zone.
    /// </summary>
    public FacilityZone? NewZone { get; }
    
    /// <summary>
    /// Gets the previous zone.
    /// </summary>
    public FacilityZone? PreviousZone { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerChangedRoomEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="newZone">The new zone.</param>
    /// <param name="previousZone">The previous zone.</param>
    public PlayerChangedZoneEventArgs(ExPlayer player, FacilityZone? newZone, FacilityZone? previousZone)
    {
        Player = player;
        NewZone = newZone;
        PreviousZone = previousZone;
    }
}