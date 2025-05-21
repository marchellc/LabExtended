using LabExtended.Extensions;

using NorthwoodLib.Pools;

using RemoteAdmin;

namespace LabExtended.Commands.Utilities;

/// <summary>
/// Used to synchronize commands with the client.
/// </summary>
public static class CommandListSynchronization
{
    /// <summary>
    /// Adds all custom commands into the specified command data list.
    /// </summary>
    /// <param name="commands">The data list.</param>
    public static void AddCommands(List<QueryProcessor.CommandData> commands)
    {
        if (commands is null)
            throw new ArgumentNullException(nameof(commands));

        if (CommandManager.Commands.Count < 1)
            return;
        
        QueryProcessor.CommandData ToData(CommandData command, CommandOverload ov)
        {
            var data = new QueryProcessor.CommandData();

            data.Command = command.Path[0];
                
            data.Hidden = command.IsHidden;
            
            data.Description = command.DefaultOverload != null && command.DefaultOverload == ov
                ? command.Description
                : ov.Description;

            data.Usage = GetUsage(command, ov);
            return data;
        }
        
        CommandManager.Commands.ForEach(cmd =>
        {
            if (cmd.DefaultOverload != null)
                commands.Add(ToData(cmd, cmd.DefaultOverload));

            foreach (var overload in cmd.Overloads)
                commands.Add(ToData(cmd, overload.Value));
        });
    }

    private static string[] GetUsage(CommandData command, CommandOverload overload)
    {
        var list = ListPool<string>.Shared.Rent();

        if (command.Path.Count > 1)
        {
            for (var i = 1; i < command.Path.Count; i++)
            {
                list.Add(command.Path[i]);
            }
        }
        
        if (command.DefaultOverload != null && command.DefaultOverload == overload)
        {
            foreach (var parameter in overload.Parameters)
            {
                if (parameter.UsageAlias?.Length > 0)
                {
                    list.Add(parameter.UsageAlias);
                }
                else if (parameter.FriendlyAlias?.Length > 0)
                {
                    list.Add(parameter.FriendlyAlias);
                }
                else
                {
                    list.Add(parameter.Name);
                }
            }
        }
        else
        {
            list.Add(overload.Name);
            
            foreach (var parameter in overload.Parameters)
            {
                if (parameter.UsageAlias?.Length > 0)
                {
                    list.Add(parameter.UsageAlias);
                }
                else if (parameter.FriendlyAlias?.Length > 0)
                {
                    list.Add(parameter.FriendlyAlias);
                }
                else
                {
                    list.Add(parameter.Name);
                }
            }
        }

        return ListPool<string>.Shared.ToArrayReturn(list);
    }
}