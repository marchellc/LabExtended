using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a player starts playing the Snake minigame on their Chaos keycard.
/// </summary>
public class PlayerSnakeStartedEventArgs : EventArgs
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
    /// Creates a new <see cref="PlayerSnakeStartedEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="keycard">The keycard item.</param>
    /// <param name="engine">The snake engine.</param>
    public PlayerSnakeStartedEventArgs(ExPlayer player, ChaosKeycardItem keycard, SnakeEngine engine)
    {
        Player = player;
        Keycard = keycard;
        Engine = engine;
    }
}