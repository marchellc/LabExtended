using InventorySystem.Items.Keycards;
using LabExtended.Core;
using UnityEngine;

namespace LabExtended.API;

/// <summary>
/// Contains information about a specific player's snake minigame.
/// </summary>
public class SnakeInfo
{
    /// <summary>
    /// Gets the starting length of the snake.
    /// </summary>
    public const byte StartLength = 5;
    
    /// <summary>
    /// Gets the size of the game area.
    /// </summary>
    public static Vector2Int AreaSize { get; } = new(18, 11);

    /// <summary>
    /// Gets the starting position.
    /// </summary>
    public static Vector2Int StartPosition { get; } = AreaSize / 2;

    internal bool deltaReceived;
    internal bool syncReceived;
    internal bool eventCalled;
    
    /// <summary>
    /// Gets the target player.
    /// </summary>
    public ExPlayer Player { get; }
    
    /// <summary>
    /// Gets the target keycard.
    /// </summary>
    public ChaosKeycardItem? Keycard { get; internal set; }
    
    /// <summary>
    /// Whether or not the player is currently playing the snake minigame.
    /// </summary>
    public bool IsPlaying { get; internal set; }

    /// <summary>
    /// Gets the player's current score.
    /// </summary>
    public int Score { get; internal set; } = 0;

    /// <summary>
    /// Gets the length of the player's snake.
    /// </summary>
    public int Length { get; internal set; } = 0;
    
    /// <summary>
    /// Gets the player's current move direction.
    /// </summary>
    public Vector2Int Direction { get; internal set; }
    
    /// <summary>
    /// Gets the player's food position.
    /// </summary>
    public Vector2Int FoodPosition { get; internal set; }
    
    internal SnakeInfo(ExPlayer target)
    {
        Player = target;
        Reset(false, true);
    }

    internal void Reset(bool isPlaying, bool resetSync)
    {
        Score = 0;
        
        Length = StartLength;

        Direction = Vector2Int.right;
        FoodPosition = Vector2Int.zero;
        
        IsPlaying = isPlaying;

        if (!isPlaying)
            Keycard = null;

        if (resetSync)
        {
            deltaReceived = false;
            syncReceived = false;
            eventCalled = false;
        }
    }
}