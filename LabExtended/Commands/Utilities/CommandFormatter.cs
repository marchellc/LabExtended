using LabExtended.Extensions;

using NorthwoodLib.Pools;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Formats command information.
/// </summary>
public static class CommandFormatter
{
    /// <summary>
    /// Appends custom commands to the selected string.
    /// </summary>
    /// <param name="str">The string to append the commands to.</param>
    public static void AppendCommands(ref string str)
    {
        if (str is null)
            throw new ArgumentNullException(nameof(str));

        if (CommandManager.Commands.Count < 1)
            return;
        
        str += StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine();
            
            foreach (var command in CommandManager.Commands)
            {
                if (command.DefaultOverload != null)
                {
                    x.Append($"{command.Name} ({command.Description})");

                    if (command.Aliases.Count > 0)
                        x.Append($" (aliases: {string.Join(", ", command.Aliases)})");
                    
                    x.AppendLine();
                }

                foreach (var overload in command.Overloads)
                    x.AppendLine($"{command.Name} {overload.Key} ({overload.Value.Description})");
            }
        });
    }
    
    /// <summary>
    /// Converts the command into an info-help string.
    /// </summary>
    /// <param name="commandData">The command instance.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetString(this CommandData commandData)
    {
        if (commandData is null)
            throw new ArgumentNullException(nameof(commandData));
        
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"[COMMAND \"{commandData.Name}\"]");
            x.AppendLine($"- Description: {commandData.Description}");
            x.AppendLine($"- Type: {commandData.Type.FullName}");

            if (commandData.Permission != null)
                x.AppendLine($"- Permission: {commandData.Permission}");

            if (commandData.Aliases.Count > 0)
                x.AppendLine($"- Aliases: {string.Join(", ", commandData.Aliases)}");

            x.AppendLine($"- Supports Remote Admin: {commandData.SupportsRemoteAdmin}");
            x.AppendLine($"- Supports Server Console: {commandData.SupportsServer}");
            x.AppendLine($"- Supports Player Console: {commandData.SupportsPlayer}");

            x.AppendLine($"- Can Be Continued: {commandData.IsContinuable}");

            if (commandData.TimeOut.HasValue)
                x.AppendLine($"- Time Out: {commandData.TimeOut.Value}s");

            void AppendOverload(string overloadHeader, CommandOverload overload)
            {
                x.AppendLine();
                x.AppendLine($"-> {overloadHeader}");

                if (string.IsNullOrWhiteSpace(overload.Name) || (commandData.DefaultOverload != null && overload == commandData.DefaultOverload))
                    x.Append($" -< Usage: \"{commandData.Name}");
                else
                    x.Append($" -< Usage: \"{commandData.Name} {overload.Name}");

                if (overload.Parameters.Count > 0)
                {
                    foreach (var parameter in overload.Parameters)
                    {
                        if (parameter.HasDefault)
                        {
                            x.Append($" ({parameter.Name})");
                        }
                        else
                        {
                            x.Append($" [{parameter.Name}]");
                        }
                    }
                }

                x.AppendLine("\"");

                if (overload.ParameterCount > 0)
                {
                    x.AppendLine($" -< Parameters ({overload.ParameterCount}):");

                    for (var y = 0; y < overload.Parameters.Count; y++)
                    {
                        var parameter = overload.Parameters[y];

                        x.Append($"  -< [{y}] {parameter.Name} ({parameter.Description})");

                        if (parameter.HasDefault)
                            x.Append($" (default: {parameter.DefaultValue?.ToString() ?? "null"})");
                        
                        foreach (var restriction in parameter.Restrictions)
                            x.Append($"\n   --> Restriction: {restriction}");
                        
                        x.AppendLine();
                    }
                }
            }

            if (commandData.DefaultOverload != null)
                AppendOverload("Default Overload", commandData.DefaultOverload);
            
            foreach (var overload in commandData.Overloads)
                AppendOverload($"Overload \"{overload.Key}\"", overload.Value);
        });
    }
}