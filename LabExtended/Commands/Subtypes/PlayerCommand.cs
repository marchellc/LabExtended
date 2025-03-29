using LabExtended.API;
using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Subtypes;

/// <summary>
/// A command that's targeting a player.
/// </summary>
public abstract class PlayerCommand : CommandBase
{
    /// <summary>
    /// Invokes the command with a single target.
    /// </summary>
    /// <param name="target">The target.</param>
    public virtual void InvokeSingle(
        [CommandParameter("Target", "The targeted player.")] ExPlayer target) { }

    /// <summary>
    /// Invokes the command with multiple targets.
    /// </summary>
    /// <param name="targets">The list of targets.</param>
    public virtual void InvokeMany(
        [CommandParameter("Targets", "List of targeted players.")]
        List<ExPlayer> targets) { }
}