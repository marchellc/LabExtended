using LabExtended.API;
using LabExtended.API.Containers;

using LabExtended.Commands.Attributes;
using LabExtended.Commands.Interfaces;

using LabExtended.Extensions;

namespace LabExtended.Commands.Custom.Debug.Switch;

/// <summary>
/// Used to get / set player switches.
/// </summary>
[Command("debug toggles", "Gets / sets the value of player toggles.")]
public class SwitchCommand : CommandBase, IServerSideCommand
{
    /// <summary>
    /// Sets a player's switch.
    /// </summary>
    [CommandOverload("set", "Sets the value of player toggles.")]
    public void SetToggle(
        [CommandParameter("Target", "The targeted player")] ExPlayer target,
        [CommandParameter("Property", "The property to set")] string property,
        [CommandParameter("Value", "The value of the player toggle.")] bool value)
    {
        var prop = typeof(SwitchContainer).FindProperty(p => p.SetMethod != null
                                                             && string.Equals(property, p.Name,
                                                                 StringComparison.InvariantCultureIgnoreCase));

        if (prop is null)
        {
            Fail($"Property \"{property}\" could not be found.");
            return;
        }
        
        if (prop.PropertyType != typeof(bool))
        {
            Fail($"Only boolean properties can be changed via commands.");
            return;
        }

        if ((bool)prop.GetValue(target.Toggles) == value)
        {
            Fail($"Property \"{prop.Name}\" is already set to {value} (for player \"{target.Nickname} ({target.UserId})\").");
            return;
        }
        
        prop.SetValue(target.Toggles, value);
        
        Ok($"Property \"{prop.Name}\" was set to \"{value}\" for player \"{target.Nickname} ({target.UserId})\"");
    }
    
    /// <summary>
    /// Sets a player's switch.
    /// </summary>
    [CommandOverload("get", "Gets the value of player toggles.")]
    public void GetToggle(
        [CommandParameter("Target", "The targeted player")] ExPlayer target,
        [CommandParameter("Property", "The property to set")] string property)
    {
        var prop = typeof(SwitchContainer).FindProperty(p => p.SetMethod != null
                                                             && string.Equals(property, p.Name,
                                                                 StringComparison.InvariantCultureIgnoreCase));

        if (prop is null)
        {
            Fail($"Property \"{property}\" could not be found.");
            return;
        }
        
        if (prop.PropertyType != typeof(bool))
        {
            Fail($"Only boolean properties can be retrieved via commands.");
            return;
        }
        
        var value = (bool)prop.GetValue(target.Toggles);
        
        Ok($"Property \"{prop.Name}\" is {(value ? "ENABLED" : "DISABLED")} for player \"{target.Nickname} ({target.UserId})\"");
    }
}