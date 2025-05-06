using LabExtended.API;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player starts playing the Snake minigame on their Chaos keycard.
/// </summary>
public class PlayerSnakeStartedEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeStartedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerSnakeStartedEventArgs(ExPlayer player) : base(player, player.Inventory.Snake) { }
}