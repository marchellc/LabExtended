using LabExtended.API;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player's snake minigame ends with a game-over.
/// </summary>
public class PlayerSnakeGameOverEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeGameOverEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerSnakeGameOverEventArgs(ExPlayer player) : base(player, player.Inventory.Snake) { }
}