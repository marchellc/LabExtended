using LabApi.Features.Enums;

using NorthwoodLib.Pools;

namespace LabExtended.Commands;

/// <summary>
/// A class used to search for commands.
/// </summary>
public static class CommandSearch
{
    /// <summary>
    /// Attempts to find a matching command overload based on the provided argument list.
    /// </summary>
    /// <remarks>The method performs a case-insensitive comparison of the argument list against each
    /// overload's path. If a match is found, the corresponding path elements are removed from the argument list, and
    /// the matching overload is returned in the out parameter.</remarks>
    /// <param name="args">The list of input arguments to match against the command's overload paths. The method removes the matched path
    /// elements from this list if a matching overload is found.</param>
    /// <param name="command">The command data containing the available overloads to search.</param>
    /// <param name="foundOverload">When this method returns, contains the matching command overload if a match is found; otherwise, null. This
    /// parameter is passed uninitialized.</param>
    /// <returns>true if a matching overload is found; otherwise, false.</returns>
    public static bool TryGetOverload(List<string> args, CommandData command, out CommandOverload? foundOverload)
    {
        foundOverload = null;

        if (args.Count < 1)
            return false;

        for (var i = 0; i < command.Overloads.Count; i++)
        {
            var overload = command.Overloads[i];

            if (args.Count >= overload.Path.Count)
            {
                var matched = true;

                for (var x = 0; x < overload.Path.Count; x++)
                {
                    if (string.Equals(overload.Path[x], args[x], StringComparison.OrdinalIgnoreCase))
                        continue;

                    matched = false;
                    break;
                }

                if (matched)
                {
                    args.RemoveRange(0, overload.Path.Count);

                    foundOverload = overload;
                    return true;
                }
            }
        }

        return false;
    }

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