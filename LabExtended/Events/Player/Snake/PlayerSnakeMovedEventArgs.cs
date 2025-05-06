using LabExtended.API;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player's Snake moves.
/// </summary>
public class PlayerSnakeMovedEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeMovedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerSnakeMovedEventArgs(ExPlayer player) : base(player, player.Inventory.Snake) { }
}