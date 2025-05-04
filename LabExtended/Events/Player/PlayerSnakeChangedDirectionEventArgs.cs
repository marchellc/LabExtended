using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player;

/// <summary>
/// Gets called when a player's snake (the Chaos Keycard minigame) changes direction.
/// </summary>
public class PlayerSnakeChangedDirectionEventArgs : EventArgs
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
    /// Gets the current direction.
    /// </summary>
    public Vector2Int? CurDirection { get; }
    
    /// <summary>
    /// Gets the new direction.
    /// </summary>
    public Vector2Int NewDirection { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSnakeChangedDirectionEventArgs"/> event.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="keycard">The keycard item.</param>
    /// <param name="engine">The snake engine.</param>
    /// <param name="curDirection">The current direction.</param>
    /// <param name="newDirection">The new direction.</param>
    public PlayerSnakeChangedDirectionEventArgs(ExPlayer player, ChaosKeycardItem keycard, SnakeEngine engine,
        Vector2Int? curDirection, Vector2Int newDirection)
    {
        Player = player;
        Keycard = keycard;
        Engine = engine;
        CurDirection = curDirection;
        NewDirection = newDirection;
    }
}