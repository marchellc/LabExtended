using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a player reaches a food segment in the Snake minigame.
/// </summary>
public class PlayerSnakeEatenEventArgs : EventArgs
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
    /// Gets the position of the next food segment.
    /// </summary>
    public Vector2Int? NextFoodPosition { get; }
    
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeEatenEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="keycard">The keycard item.</param>
    /// <param name="engine">The snake engine.</param>
    /// <param name="nextFoodPosition">Next food segment position.</param>
    public PlayerSnakeEatenEventArgs(ExPlayer player, ChaosKeycardItem keycard, SnakeEngine engine, Vector2Int? nextFoodPosition)
    {
        Player = player;
        Keycard = keycard;
        Engine = engine;
        NextFoodPosition = nextFoodPosition;
    }
}