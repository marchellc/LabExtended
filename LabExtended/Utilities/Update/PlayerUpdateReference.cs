using LabExtended.API.Enums;
using LabExtended.Extensions;

namespace LabExtended.Utilities.Update;

/// <summary>
/// Represents a handle in <see cref="PlayerUpdateHelper.Component.OnUpdate"/>.
/// </summary>
public class PlayerUpdateReference
{
    internal PlayerUpdateReference() { }
    
    /// <summary>
    /// Remaining delay.
    /// </summary>
    public float RemainingTime { get; internal set; } = 0f;
    
    /// <summary>
    /// Default delay.
    /// </summary>
    public float DelayTime { get; set; } = 0f;

    /// <summary>
    /// Whether or not the update is enabled.
    /// </summary>
    public bool IsEnabled { get; internal set; }

    /// <summary>
    /// Gets or sets the blacklisted round states.
    /// </summary>
    public RoundState? BlacklistedStates { get; set; } = null;
    
    /// <summary>
    /// Gets or sets the whitelisted round states.
    /// </summary>
    public RoundState? WhitelistedStates { get; set; } = null;
    
    /// <summary>
    /// Gets the compiled method to call during OnUpdate.
    /// </summary>
    public Action? OnUpdate { get; internal set; }
    
    /// <summary>
    /// Gets the target method to call.
    /// </summary>
    public Action? TargetUpdate { get; internal set; }

    /// <summary>
    /// Disables this handle.
    /// </summary>
    public void Disable()
        => IsEnabled = false;

    /// <summary>
    /// Enables this handle.
    /// </summary>
    public void Enable()
        => IsEnabled = true;

    /// <summary>
    /// Toggles this handle.
    /// </summary>
    public void Toggle()
        => IsEnabled = !IsEnabled;

    /// <inheritdoc cref="object.ToString"/>
    public override string ToString()
        => $"PlayerUpdate [{TargetUpdate?.Method?.GetMemberName() ?? "null target method"}] " +
           $"(IsEnabled={IsEnabled}, DelayTime={DelayTime}, RemainingTime={RemainingTime})";
}