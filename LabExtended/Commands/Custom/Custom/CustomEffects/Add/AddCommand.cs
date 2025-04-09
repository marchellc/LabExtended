using LabExtended.API;
using LabExtended.API.CustomEffects;
using LabExtended.API.CustomEffects.SubEffects;

using LabExtended.Commands.Attributes;

namespace LabExtended.Commands.Custom.Custom.CustomEffects;

public partial class CustomEffectsCommand
{
    [CommandOverload("add", "Adds a custom effect to a list of players.")]
    public void AddOverload(
        [CommandParameter("Targets", "Players to add the effect to.")] List<ExPlayer> targets,
        [CommandParameter("Name", "Name of the effect to apply.")] string effectName,
        [CommandParameter("Duration", "Duration of the effect (if supported).")] float? effectDuration = null)
    {
        if (!CustomEffect.TryGetEffect(effectName, false, true, out var effectType))
        {
            Fail($"Unknown custom effect: \"{effectName}\"");
            return;
        }

        Ok(x =>
        {
            targets.ForEach(t =>
            {
                if (!t.Effects.CustomEffects.TryGetValue(effectType, out var customEffect))
                {
                    if (Activator.CreateInstance(effectType) is not CustomEffect createdEffect)
                    {
                        x.AppendLine(
                            $"[{t.Nickname} ({t.ClearUserId})]: Could not construct effect \"{effectType.FullName}\"");
                        return;
                    }

                    customEffect = createdEffect;
                    
                    t.Effects.CustomEffects.Add(effectType, customEffect);
                    
                    customEffect.Player = t;
                    customEffect.Start();
                }

                if (customEffect.IsActive)
                {
                    x.AppendLine($"[{t.Nickname} ({t.ClearUserId})]: Effect \"{effectType.Name}\" is already active");
                    return;
                }

                if (customEffect is DurationCustomEffect durationCustomEffect)
                {
                    if (effectDuration.HasValue)
                        durationCustomEffect.Remaining = effectDuration.Value;

                    x.AppendLine(
                        $"[{t.Nickname} ({t.ClearUserId})]: Effect \"{effectType.Name}\" was enabled for {durationCustomEffect.Remaining}s");
                    return;
                }
                
                customEffect.Enable();

                x.AppendLine($"[{t.Nickname} ({t.ClearUserId})]: Effect \"{effectType.Name}\" was enabled.");
            });
        });
    }
}