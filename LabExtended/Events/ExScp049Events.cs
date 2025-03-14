using LabExtended.Events.Scp049;
using LabExtended.Extensions;

#pragma warning disable CS8604 // Possible null reference argument.

namespace LabExtended.Events;

/// <summary>
/// SCP-049 related events.
/// </summary>
public static class ExScp049Events
{
    /// <inheritdoc cref="Scp049SensingTargetEventArgs"/>
    public static event Action<Scp049SensingTargetEventArgs>? SensingTarget; 
    
    /// <inheritdoc cref="Scp049CancellingResurrectionEventArgs"/>
    public static event Action<Scp049CancellingResurrectionEventArgs>? CancellingResurrection; 
    
    /// <inheritdoc cref="Scp049AttemptingResurrectionEventArgs"/>
    public static event Action<Scp049AttemptingResurrectionEventArgs>? AttemptingResurrection;

    /// <summary>
    /// Invokes the <see cref="SensingTarget"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnSensingTarget(Scp049SensingTargetEventArgs args)
        => SensingTarget.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="CancellingResurrection"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnCancellingResurrection(Scp049CancellingResurrectionEventArgs args)
        => CancellingResurrection.InvokeBooleanEvent(args);
    
    /// <summary>
    /// Invokes the <see cref="AttemptingResurrection"/> event.
    /// </summary>
    /// <param name="args">The event's arguments.</param>
    /// <returns>The <see cref="BooleanEventArgs.IsAllowed"/> property.</returns>
    public static bool OnAttemptingResurrection(Scp049AttemptingResurrectionEventArgs args)
        => AttemptingResurrection.InvokeBooleanEvent(args);
}