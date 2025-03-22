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
                x.Append($"[{command.Name.ToUpper()}] {command.Description} (");

                if (command.SupportsPlayer)
                {
                    x.Append("PLY");
                }

                if (command.SupportsServer)
                {
                    if (command.SupportsPlayer)
                        x.Append(", ");

                    x.Append("SRV");
                }

                if (command.SupportsRemoteAdmin)
                {
                    if (command.SupportsPlayer || command.SupportsServer)
                        x.Append(", ");

                    x.Append("RA");
                }

                x.AppendLine(")");
            }
        });
    }
    
    /// <summary>
    /// Converts the command into an info-help string.
    /// </summary>
    /// <param name="commandInstance">The command instance.</param>
    /// <returns>The string.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string GetString(this CommandInstance commandInstance)
    {
        if (commandInstance is null)
            throw new ArgumentNullException(nameof(commandInstance));
        
        return StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"Command \"{commandInstance.Name}\"");
            x.AppendLine($"Description: {commandInstance.Description}");
            x.AppendLine($"Type: {commandInstance.Type.FullName}");

            if (commandInstance.Permission != null)
                x.AppendLine($"Permission: {commandInstance.Permission}");

            if (commandInstance.Aliases.Count > 0)
                x.AppendLine($"Aliases: {string.Join(", ", commandInstance.Aliases)}");

            x.AppendLine($"Supports Remote Admin: {commandInstance.SupportsRemoteAdmin}");
            x.AppendLine($"Supports Server Console: {commandInstance.SupportsServer}");
            x.AppendLine($"Supports Player Console: {commandInstance.SupportsPlayer}");

            x.AppendLine($"Can Be Continued: {commandInstance.IsContinuable}");

            if (commandInstance.TimeOut.HasValue)
                x.AppendLine($"Time Out: {commandInstance.TimeOut.Value}s");

            x.AppendLine($"{commandInstance.Overloads.Count} overload(s):");

            for (var i = 0; i < commandInstance.Overloads.Count; i++)
            {
                var overload = commandInstance.Overloads[i];

                x.AppendLine();
                x.AppendLine($"Overload [{i}]:");
                x.AppendLine($" Name: {overload.Name}");
                x.AppendLine($" Method: {overload.Target.GetMemberName()}");
                x.AppendLine($" Is Coroutine: {overload.IsCoroutine}");
                x.AppendLine($" Is Initialized: {overload.IsInitialized}");

                if (overload.ParameterCount > 0)
                    x.AppendLine($" Parameters ({overload.ParameterCount}):");

                for (var y = 0; y < overload.Parameters.Count; y++)
                {
                    var parameter = overload.Parameters[y];

                    x.AppendLine($"  Parameter [{y}]:");
                    x.AppendLine($"   Name: {parameter.Name}");
                    x.AppendLine($"   Description: {parameter.Description}");
                    
                    if (parameter.UsageAlias?.Length > 0)
                        x.AppendLine($"   Usage Alias: {parameter.UsageAlias}");
                    
                    if (parameter.FriendlyAlias?.Length > 0)
                        x.AppendLine($"   Friendly Alias: {parameter.FriendlyAlias}");

                    if (parameter.HasDefault)
                        x.AppendLine($"   Default Value: {parameter.DefaultValue?.ToString() ?? "null"}");

                    x.AppendLine($"   Type: {parameter.Type.Type.FullName}");
                    x.AppendLine($"    Parser: {parameter.Type.Parser?.GetType()?.FullName ?? "null"}");

                    if (parameter.Type.KeyType != null)
                        x.AppendLine($"    Key Type: {parameter.Type.KeyType.FullName}");

                    if (parameter.Type.ValueType != null)
                        x.AppendLine($"    Value Type: {parameter.Type.ValueType.FullName}");

                    x.AppendLine($"    Is String: {parameter.Type.IsString}");
                    x.AppendLine($"    Is List: {parameter.Type.IsList}");
                    x.AppendLine($"    Is Array: {parameter.Type.IsArray}");
                    x.AppendLine($"    Is Dictionary: {parameter.Type.IsDictionary}");
                }
            }
        });
    }
}