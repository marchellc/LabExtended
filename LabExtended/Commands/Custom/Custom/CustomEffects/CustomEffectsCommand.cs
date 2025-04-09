using LabExtended.API.CustomEffects;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

namespace LabExtended.Commands.Custom.Custom.CustomEffects;

[Command("customeffects", "Lists all known custom effects.", "ceffects")]
public partial class CustomEffectsCommand : CommandBase, IServerSideCommand
{
    [CommandOverload]
    public void ListOverload()
    {
        Ok(x =>
        {
            if (CustomEffect.Effects.Count < 1)
            {
                x.AppendLine("No custom effects were registered.");
                return;
            }

            x.AppendLine($"{CustomEffect.Effects.Count} custom effect(s):");

            foreach (var type in CustomEffect.Effects)
                x.AppendLine($"{type.Name} ({type.FullName})");
        });
    }
}