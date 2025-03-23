using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Debug.Get;

/// <summary>
/// A command used to toggle values of player switches.
/// </summary>
[Command("debug get switch", "Gets the value of player toggles.")]
public class SwitchCommand : CommandBase, IServerCommand
{
    /// <summary>
    /// Main overload.
    /// </summary>
    /// <param name="target">The targeted player.</param>
    /// <param name="targetSwitch">The targeted switch property.</param>
    [CommandOverload]
    public void Method(
        [CommandParameter("Target", "The targeted player.")] ExPlayer target,
        [CommandParameter("Switch", "The name of the targeted switch property (not case sensitive).")] string targetSwitch)
    {
        var targetProperty = typeof(SwitchContainer).FindProperty(
            p => string.Equals(p.Name, targetSwitch, StringComparison.InvariantCultureIgnoreCase) 
                 && p.PropertyType == typeof(bool)
                 && p.GetMethod != null);

        if (targetProperty is null)
        {
            Fail($"Unknown property: \"{targetSwitch}\"");
            return;
        }
        
        var value = targetProperty.GetValue(target.Toggles);
        
        Ok($"Value of switch \"{targetProperty.Name}\" (of player \"{target.Nickname} ({target.UserId})\" is \"{value}\".");
    }
}