using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a player resets their Snake minigame.
/// </summary>
public class PlayerSnakeResetEventArgs : EventArgs
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
    /// Creates a new <see cref="PlayerSnakeResetEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="keycard">The keycard item.</param>
    /// <param name="engine">The snake engine.</param>
    public PlayerSnakeResetEventArgs(ExPlayer player, ChaosKeycardItem keycard, SnakeEngine engine)
    {
        Player = player;
        Keycard = keycard;
        Engine = engine;
    }
}