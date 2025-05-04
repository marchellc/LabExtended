using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a player's snake minigame ends with a game-over.
/// </summary>
public class PlayerSnakeGameOverEventArgs : EventArgs
{
    /// <summary>
    /// Gets the player.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the keycard.
    /// </summary>
    public ChaosKeycardItem Keycard { get; }
    
    /// <summary>
    /// Gets the snake engine.
    /// </summary>
    public SnakeEngine Engine { get; }
    
    /// <summary>
    /// Gets the reached score.
    /// </summary>
    public int Score { get; }
    
    /// <summary>
    /// Gets the reached snake length (amount of snake segments).
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSnakeGameOverEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="keycard">The keycard item.</param>
    /// <param name="engine">The snake engine.</param>
    /// <param name="score">The reached score.</param>
    /// <param name="length">The reached length.</param>
    public PlayerSnakeGameOverEventArgs(ExPlayer player, ChaosKeycardItem keycard, SnakeEngine engine, int score,
        int length)
    {
        Player = player;
        Keycard = keycard;
        Engine = engine;
        Score = score;
        Length = length;
    }
}