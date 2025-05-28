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
    /// Creates a new <see cref="RoleSelectorResult"/> instance.
    /// </summary>
    /// <param name="scpsOverflowing">Whether or not SCPs are overflowing.</param>
    /// <param name="scpOverflowHumeShieldMultiplier">Overflowing SCPs Hume Shield multiplier.</param>
    public RoleSelectorResult(bool scpsOverflowing, float scpOverflowHumeShieldMultiplier)
    {
        ScpsOverflowing = scpsOverflowing;
        ScpOverflowHumeShieldMultiplier = scpOverflowHumeShieldMultiplier;
    }
}