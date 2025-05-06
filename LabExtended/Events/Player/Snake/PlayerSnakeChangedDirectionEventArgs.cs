using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player's snake (the Chaos Keycard minigame) changes direction.
/// </summary>
public class PlayerSnakeChangedDirectionEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Gets the new direction.
    /// </summary>
    public Vector2Int NewDirection { get; }

    /// <summary>
    /// Creates a new <see cref="PlayerSnakeChangedDirectionEventArgs"/> event.
    /// </summary>
    /// <param name="player">The target player.</param>
    /// <param name="newDirection">The new direction.</param>
    public PlayerSnakeChangedDirectionEventArgs(ExPlayer player, Vector2Int newDirection) : base(player, player.Inventory.Snake)
    {
        NewDirection = newDirection;
    }
}