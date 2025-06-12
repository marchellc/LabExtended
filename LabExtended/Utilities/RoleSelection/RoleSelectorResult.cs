using PlayerRoles;

namespace LabExtended.Utilities.RoleSelection;

/// <summary>
/// Contains the results of a role selection.
/// </summary>
public struct RoleSelectorResult
{
    /// <summary>
    /// Whether or not SCPs are overflowing.
    /// </summary>
    public bool ScpsOverflowing { get; set; }
    
    /// <summary>
    /// Gets the multiplier created by overflowing SCPs.
    /// </summary>
    public float ScpOverflowHumeShieldMultiplier { get; set; }

    /// <summary>
    /// Gets the generated human team queue.
    /// </summary>
    public Team[] HumanTeamQueue { get; set; }
    
    /// <summary>
    /// Gets the generated human team queue length.
    /// </summary>
    public int HumanTeamQueueLength { get; set; }

    /// <summary>
    /// Creates a new <see cref="RoleSelectorResult"/> instance.
    /// </summary>
    /// <param name="scpsOverflowing">Whether or not SCPs are overflowing.</param>
    /// <param name="scpOverflowHumeShieldMultiplier">Overflowing SCPs Hume Shield multiplier.</param>
    /// <param name="humanTeamQueue">Human team queue.</param>
    /// <param name="humanTeamQueueLength">Human team queue length.</param>
    public RoleSelectorResult(bool scpsOverflowing, float scpOverflowHumeShieldMultiplier, Team[] humanTeamQueue, int humanTeamQueueLength)
    {
        ScpsOverflowing = scpsOverflowing;
        ScpOverflowHumeShieldMultiplier = scpOverflowHumeShieldMultiplier;
        
        HumanTeamQueue = humanTeamQueue;
        HumanTeamQueueLength = humanTeamQueueLength;
    }
}