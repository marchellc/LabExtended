using InventorySystem.Items.Keycards;
using InventorySystem.Items.Keycards.Snake;

using LabExtended.API;

using UnityEngine;

namespace LabExtended.Events.Player.Snake;

/// <summary>
/// Gets called when a player reaches a food segment in the Snake minigame.
/// </summary>
public class PlayerSnakeEatenEventArgs : PlayerSnakeEventArgs
{
    /// <summary>
    /// Creates a new <see cref="PlayerSnakeEatenEventArgs"/> instance.
    /// </summary>
    /// <param name="player">The player.</param>
    public PlayerSnakeEatenEventArgs(ExPlayer player) : base(player, player.Inventory.Snake) { }
}