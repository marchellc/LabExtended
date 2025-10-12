using LabExtended.API;
using LabExtended.API.Custom.Effects;
using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.CustomEffects;

[Command("customeffect", "Manages Custom Effects", "ceffect")]
public class CustomEffectsCommand : CommandBase, IServerSideCommand
{
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