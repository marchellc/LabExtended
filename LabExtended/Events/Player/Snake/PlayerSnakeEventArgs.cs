using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Base class for all Snake events
/// </summary>
public class PlayerSnakeEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the snake info wrapper.
    /// </summary>
    public SnakeInfo Snake { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSnakeEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="snake">The player's snake info wrapper.</param>
    public PlayerSnakeEventArgs(ExPlayer player, SnakeInfo snake)
    {
        Player = player;
        Snake = snake;
    }
}