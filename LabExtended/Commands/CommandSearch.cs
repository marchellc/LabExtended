using LabApi.Features.Enums;
using NorthwoodLib.Pools;

namespace LabExtended.Commands;

/// <summary>
/// A class used to search for commands.
/// </summary>
public static class CommandSearch
{
    /// <summary>
    /// Attempts to find a command for a list of arguments.
    /// </summary>
    /// <param name="args">The argument list.</param>
    /// <param name="commandType">The used console type.</param>
    /// <param name="likelyCommands">A list of likely commands if the command was not found.</param>
    /// <param name="foundCommand">The found command instance.</param>
    /// <returns>true if the command was found</returns>
    public static bool TryGetCommand(List<string> args, CommandType? commandType, out List<CommandData>? likelyCommands, out CommandData? foundCommand)
    {
        foundCommand = null;
        likelyCommands = null;
        
        if (args.Count < 1)
            return false;
        
        for (var i = 0; i < CommandManager.Commands.Count; i++)
        {
            var command = CommandManager.Commands[i];

            if (commandType is CommandType.Client && !command.SupportsPlayer)
                continue;

            if (commandType is CommandType.Console && !command.SupportsServer)
                continue;
            
            if (commandType is CommandType.RemoteAdmin && !command.SupportsRemoteAdmin)
                continue;

            if (args.Count < command.Path.Count)
            {
                var count = 0;
                
                for (var x = 0; x < command.Path.Count; x++)
                {
                    if (x < args.Count && string.Equals(command.Path[x], args[x], StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        continue;
                    }

                    if (x + 1 < args.Count && x + 1 >= command.Path.Count
                        && command.Aliases.Any(alias =>
                            string.Equals(alias, args[x], StringComparison.OrdinalIgnoreCase)))
                    {
                        count++;
                        continue;
                    }
                    
                    break;
                }
                
                if (count > 0)
                {
                    likelyCommands ??= ListPool<CommandData>.Shared.Rent();

                    if (!likelyCommands.Contains(command))
                        likelyCommands.Add(command);
                }
            }
            else
            {
                var matched = true;
                var count = 0;

                for (var x = 0; x < command.Path.Count; x++)
                {
                    if (string.Equals(command.Path[x], args[x], StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        continue;
                    }

                    if (x + 1 >= command.Path.Count
                        && command.Aliases.Any(alias =>
                            string.Equals(alias, args[x], StringComparison.OrdinalIgnoreCase)))
                    {
                        count++;
                        continue;
                    }

                    matched = false;
                    break;
                }

                if (matched)
                {
                    if (likelyCommands != null)
                        ListPool<CommandData>.Shared.Return(likelyCommands);

                    likelyCommands = null;

                    args.RemoveRange(0, command.Path.Count);

                    foundCommand = command;
                    return true;
                }

                if (count > 0)
                {
                    likelyCommands ??= ListPool<CommandData>.Shared.Rent();

                    if (!likelyCommands.Contains(command))
                        likelyCommands.Add(command);
                }
            }
        }

        return false;
    }
}