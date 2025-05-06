using LabExtended.API;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player stops playing the Snake minigame on their Chaos keycard (either by throwing the item, de-selecting it
/// or canceling inspection).
/// </summary>
public class PlayerSnakeStoppedEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeStoppedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerSnakeStoppedEventArgs(ExPlayer player) : base(player, player.Inventory.Snake) { }
}