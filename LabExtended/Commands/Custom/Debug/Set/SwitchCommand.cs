using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Debug.Set;

/// <summary>
/// A command used to toggle values of player switches.
/// </summary>
[Command("debug set switch", "Sets player toggles.")]
public class SwitchCommand : CommandBase, IServerCommand
{
    /// <summary>
    /// Main overload.
    /// </summary>
    /// <param name="target">The targeted player.</param>
    /// <param name="targetSwitch">The targeted switch property.</param>
    /// <param name="switchValue">The value of the switch.</param>
    [CommandOverload]
    public void Overload(
        [CommandParameter("Target", "The targeted player.")] ExPlayer target,
        [CommandParameter("Switch", "The name of the targeted switch property (not case sensitive).")] string targetSwitch, 
        [CommandParameter("Value", "The new value of the switch.")] bool switchValue)
    {
        var targetProperty = typeof(SwitchContainer).FindProperty(
            p => string.Equals(p.Name, targetSwitch, StringComparison.InvariantCultureIgnoreCase) 
                 && p.PropertyType == typeof(bool)
                 && p.SetMethod != null);

        if (targetProperty is null)
        {
            Fail($"Unknown property: \"{targetSwitch}\"");
            return;
        }
        
        targetProperty.SetValue(target.Toggles, switchValue);
        
        Ok($"Set value of property \"{targetProperty.Name}\" (for player \"{target.Nickname} ({target.UserId})\") to \"{switchValue}\".");
    }
}