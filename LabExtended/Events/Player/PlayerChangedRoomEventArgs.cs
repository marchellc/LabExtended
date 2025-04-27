using LabExtended.API;

using MapGeneration;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called after a room change is detected.
/// </summary>
public class PlayerChangedRoomEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player who changed rooms.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the new room.
    /// </summary>
    public RoomIdentifier? NewRoom { get; }
    
    /// <summary>
    /// Gets the previous room.
    /// </summary>
    public RoomIdentifier? PreviousRoom { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerChangedRoomEventArgs"/>
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="newRoom">The new room.</param>
    /// <param name="previousRoom">The previosu room.</param>
    public PlayerChangedRoomEventArgs(ExPlayer player, RoomIdentifier? newRoom, RoomIdentifier? previousRoom)
    {
        Player = player;
        NewRoom = newRoom;
        PreviousRoom = previousRoom;
    }
}