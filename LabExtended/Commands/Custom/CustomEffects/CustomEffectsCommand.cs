using LabExtended.API;
using LabExtended.API.Custom.Effects;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomEffects;

/// <summary>
/// Provides server-side commands for listing, enabling, disabling, and clearing custom effects on players.
/// </summary>
/// <remarks>This command group allows administrators or authorized users to manage custom effects for themselves
/// or other players. Use the available subcommands to view all registered effects, enable or disable specific effects
/// for a player, or clear all active custom effects. All operations are performed server-side and require appropriate
/// permissions to execute.</remarks>
[Command("customeffect", "Manages Custom Effects", "ceffect")]
public class CustomEffectsCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Lists all available custom effects, or the custom effects registered on a specified player.
    /// </summary>
    /// <param name="target">The target player whose registered custom effects will be listed. If null, lists all available custom effects
    /// instead.</param>
    [CommandOverload("list", "Lists all available Custom Effects.", null)]
    public void ListCommand(
        [CommandParameter("Target", "The target player. Specifying a " +
                                    "target will list of effects registered on a player.")] ExPlayer? target = null)
    {
        if (target != null)
        {
            Ok(x =>
            {
                x.AppendLine();
                x.AppendLine(
                    $"Showing {target.Effects.CustomEffects.Count} registered Custom Effect(s) on \"{target.Nickname}\" ({target.ClearUserId}).");

                foreach (var pair in target.Effects.CustomEffects)
                {
                    x.Append($" - {pair.Key.Name} (Active: {pair.Value.IsActive}");

                    if (pair.Value.IsActive && pair.Value is CustomDurationEffect durationEffect)
                        x.Append($"; Remaining: {durationEffect.RemainingDuration}s");

                    x.AppendLine(")");
                }
            });
        }
        else
        {
            Ok(x =>
            {
                x.AppendLine();
                x.AppendLine($"Showing {CustomPlayerEffect.Effects.Count} registered Custom Effect(s)");

                foreach (var type in CustomPlayerEffect.Effects)
                    x.AppendLine($" - {type.Name}");
            });
        }
    }
    
    /// <summary>
    /// Enables a previously registered but inactive Custom Effect for a player.
    /// </summary>
    /// <remarks>If the specified effect is already active or not registered for the target player, the
    /// command will not enable it and will report an error. Use the "customeffect list" command to view available
    /// effects.</remarks>
    /// <param name="effectName">The name of the Custom Effect to enable. Must correspond to a registered effect.</param>
    /// <param name="target">The player on whom to enable the effect. If not specified, the effect is enabled on the command sender.</param>
    [CommandOverload("enable", "Enables an inactive Custom Effect.", null)]
    public void EnableCommand(
        [CommandParameter("Name", "Name of the Custom Effect.")] string effectName, 
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        if (!CustomPlayerEffect.TryGetEffect(effectName, false, true, out var effectType))
        {
            Fail($"Unknown effect: \"{effectName}\".\nUse \"customeffect list\" to get a list of available effects.");
            return;
        }

        var player = target ?? Sender;

        if (!player.Effects.CustomEffects.TryGetValue(effectType, out var effect))
        {
            Fail($"Player \"{player.Nickname}\" ({player.ClearUserId}) does not have the specified Custom Effect registered.");
            return;
        }

        if (effect.IsActive)
        {
            Fail($"Effect \"{effect.GetType().Name}\" is already active on player \"{player.Nickname}\" ({player.ClearUserId}).");
            return;
        }
        
        effect.Enable();
        
        Ok($"Enabled effect \"{effect.GetType().Name}\" on \"{player.Nickname}\" ({player.ClearUserId}).");
    }

    /// <summary>
    /// Disables an active Custom Effect for the specified player.
    /// </summary>
    /// <remarks>If the specified effect is not active or not registered for the target player, the command
    /// will fail with an appropriate message. Use the "customeffect list" command to view available effects.</remarks>
    /// <param name="effectName">The name of the Custom Effect to disable. This must match the name of an existing effect.</param>
    /// <param name="target">The player on whom to disable the effect. If not specified, the effect is disabled on the command sender.</param>
    [CommandOverload("disable", "Disables an active Custom Effect.", null)]
    public void DisableCommand(
        [CommandParameter("Name", "Name of the Custom Effect")] string effectName, 
        [CommandParameter("Target", "The target player (defaults to you).")] ExPlayer? target = null)
    {
        if (!CustomPlayerEffect.TryGetEffect(effectName, false, true, out var effectType))
        {
            Fail($"Unknown effect: \"{effectName}\".\nUse \"customeffect list\" to get a list of available effects.");
            return;
        }

        var player = target ?? Sender;

        if (!player.Effects.CustomEffects.TryGetValue(effectType, out var effect))
        {
            Fail($"Player \"{player.Nickname}\" ({player.ClearUserId}) does not have the specified Custom Effect registered.");
            return;
        }

        if (!effect.IsActive)
        {
            Fail($"Effect \"{effect.GetType().Name}\" is not active on player \"{player.Nickname}\" ({player.ClearUserId}).");
            return;
        }
        
        effect.Disable();
        
        Ok($"Disabled effect \"{effect.GetType().Name}\" on \"{player.Nickname}\" ({player.ClearUserId}).");
    }

    /// <summary>
    /// Disables all active custom effects for the specified player.
    /// </summary>
    /// <param name="target">The player whose custom effects will be cleared. If null, the command sender is used.</param>
    [CommandOverload("clear", "Clears all Custom Effects.", null)]
    public void ClearCommand(ExPlayer? target = null)
    {
        var player = target ?? Sender;
        var count = 0;

        foreach (var pair in player.Effects.CustomEffects)
        {
            if (pair.Value.IsActive)
            {
                pair.Value.Disable();
                count++;
            }
        }
        
        Ok($"Disabled {count} enabled Custom Effects on \"{player.Nickname}\" ({player.ClearUserId}).");
    }
}